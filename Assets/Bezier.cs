using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bezier : MonoBehaviour
{
    List<Transform> controlPoints = new List<Transform>();
    List<GameObject> samples = new List<GameObject>();
    public GameObject sampleObjectPrefab;
    public GameObject controlPointPrefab;

    public float discreteSampleDistance = 0.5f;

    public bool outgoing = true;
    public float speed = 2.5f;
    float maxDistance = 10;
    float curDistance = 0;

    Vector3 moveDir = Vector3.forward;
    public void Cast(Player caster, Vector3 target) {
        lastPos = this.transform.position;
        controlPoints.Add(this.transform); //add head
        GameObject tail = SpawnControlPoint(); 
        tail.transform.SetParent(caster.transform);
        
        Vector3 dir = (target-caster.transform.position).normalized * speed;
        this.GetComponent<Rigidbody>().AddForce(dir, ForceMode.Impulse);
    }

    GameObject SpawnControlPoint() {
        // if(controlPoints.Count > 1) return;
        GameObject go = GameObject.Instantiate(controlPointPrefab, this.transform.position, Quaternion.identity);
        controlPoints.Insert(1, go.transform);
        return go;
    }

    GameObject GetOrCreateSampledPoint(int i) {
        if(i < samples.Count) {            
            return samples[i];
        }
        else {
            GameObject go = GameObject.Instantiate(sampleObjectPrefab, Vector3.zero, Quaternion.identity);
            samples.Add(go);
            return go;
        }
    }
    // Update is called once per frame
    void Update()
    {
        

        //head
        curDistance += (this.transform.position - lastPos).magnitude;
        lastPos = this.transform.position;

        if(outgoing && curDistance > maxDistance) {
            outgoing = false;
            this.GetComponent<SphereCollider>().enabled = false;
        }

        if(!outgoing) {
            if(controlPoints.Count <= 1) {
                // //cleanup extra samples
                for(int x=samples.Count-1; x>=0; x--) {                
                    GameObject sample = samples[x];
                    samples.Remove(sample);
                    GameObject.Destroy(sample);
                }
                GameObject.Destroy(this.gameObject);
            }
            else {
                Transform prevControlPoint = controlPoints[1];
                Vector3 toPrevControlPoint = (prevControlPoint.position - this.transform.position);
                this.GetComponent<Rigidbody>().velocity = toPrevControlPoint.normalized * speed;

                if(toPrevControlPoint.magnitude <= 0.1f) 
                {
                    GameObject.Destroy(prevControlPoint.gameObject);
                    controlPoints.RemoveAt(1);
                }

            }
        }

        if(controlPoints.Count > 1) {
            float[] bezierSegments = CalculateBezierSegments(100);
            int i = 0;
            float targetDistance = i * discreteSampleDistance;
            bool isOutsideBezier = (targetDistance > bezierSegments[bezierSegments.Length-1]);

            while(!isOutsideBezier) {                                
                GetOrCreateSampledPoint(i).transform.position = SampleDiscrete(i, bezierSegments);
                
                i++;
                targetDistance = i * discreteSampleDistance;
                isOutsideBezier = (targetDistance > bezierSegments[bezierSegments.Length-1]);
            }

            // //cleanup extra samples
            for(int x=samples.Count-1; x>=i; x--) {                
                GameObject sample = samples[x];
                samples.Remove(sample);
                GameObject.Destroy(sample);
            }
        }
    }


    // Update is called once per frame
    Vector3 lastPos;
    void FixedUpdate() {
        // this.GetComponent<Rigidbody>().velocity = new Vector3(speed, 0, -speed);

        // this.GetComponent<Rigidbody>().MovePosition(moveDir.normalized * speed * Time.fixedDeltaTime);
    }

    public void OnCollisionEnter(Collision collision) {
        if(outgoing) {
            if(collision.gameObject.GetComponent<Player>()) return;
            SpawnControlPoint();
        }
    }

    float[] CalculateBezierSegments(int resolution) {
        //non evenly spaced segments
        float[] output = new float[resolution+1];
        Vector3 lastSamplePosition = Sample(0); 
        output[0] = 0;
        float aggDistance = 0;
        for (int i = 1; i < resolution+1; i++)
        {
            float t = (float)i/(float)resolution;
            Vector3 sample = Sample(t);
            float delta = (sample - lastSamplePosition).magnitude;
            aggDistance += delta;
            lastSamplePosition = sample;
            output[i] = aggDistance;
        }

        return output;
    }

    Vector3 SampleDiscrete(int index, float[] bezierSegments) {
        float targetDistance = index * discreteSampleDistance;
        for (int i = 1; i < bezierSegments.Length; i++)
        {
            //if we find a bezier point that is further down the distance, we can try and interpolate
            if(targetDistance < bezierSegments[i]) {
                float t = (float)i/(float)(bezierSegments.Length-1);
                float tminus1 = (float)(i-1)/(float)(bezierSegments.Length-1);

                //we want to interpolate t so we find a T that gives us the target distance
                float bezDistance = bezierSegments[i];
                float prevBezDistance = bezierSegments[i-1];
                float weight = (targetDistance-prevBezDistance)/(bezDistance-prevBezDistance);
                float T = (t-tminus1)*weight + tminus1;
                
                return Sample(T);
            }
        }

        return Vector3.zero;
    }

    Vector3 Sample(float t) {
        //sample bezier curve
        Vector3[] coeffs = new Vector3[controlPoints.Count];

        for(int i=0; i<controlPoints.Count; i++) {
            coeffs[i] = controlPoints[i].position;
        }

        int n = controlPoints.Count;
        for(int i=1; i<n; i++) {
            for(int j=0; j<(n-i); j++) {
                coeffs[j] = coeffs[j] * (1-t) + coeffs[j+1] * t;
            }
        }

        return coeffs[0];
    }
}
