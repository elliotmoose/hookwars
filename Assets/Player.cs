using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    public GameObject hookHeadPrefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
            RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction);
            foreach(RaycastHit hit in hits) {
                if(hit.collider.gameObject.name == "Floor"){
                    this.GetComponent<NavMeshAgent>().SetDestination(hit.point);
                }
            }
        }

        if(Input.GetMouseButtonDown(1)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
            RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction);
            foreach(RaycastHit hit in hits) {
                if(hit.collider.gameObject.name == "Floor"){
                    CastHook(new Vector3(hit.point.x, this.transform.position.y, hit.point.z));
                }
            }
        }
    }

    void CastHook(Vector3 target) {
        float speed = 5;
        GameObject hookHead = GameObject.Instantiate(hookHeadPrefab, this.transform.position, Quaternion.identity);
        hookHead.GetComponent<Bezier>().Cast(this, target);
    }
}
