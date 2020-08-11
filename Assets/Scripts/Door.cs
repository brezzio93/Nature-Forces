using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    private Rigidbody2D m_Rigidbody2D;

    // Start is called before the first frame update
    public float speed;
    public BoxCollider2D col1, col2;

    public Vector3 basePos;

    private void Start()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        basePos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //transform.position = new Vector3(transform.position.x, transform.position.y+3, transform.position.z);
            StartCoroutine(OpenDoor(other));
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            
            StartCoroutine(CloseDoor());
        }
    }

    public IEnumerator OpenDoor(Collision2D other)
    {
        Debug.Log("OpenDoor");
        m_Rigidbody2D.velocity = new Vector2(0, Time.deltaTime * speed);
        yield return new WaitForSeconds(.5f);        
        GetComponent<BoxCollider2D>().enabled = false;
        m_Rigidbody2D.velocity = new Vector2(0, 0);
        yield return new WaitForSeconds(3f);
        col1.enabled = col2.enabled = true;
    }

    public IEnumerator CloseDoor()
    {
        Debug.Log("CloseDoor");
        m_Rigidbody2D.velocity = new Vector2(0, -Time.deltaTime * speed);
        col1.enabled = col2.enabled = false;
        GetComponent<BoxCollider2D>().enabled = true;
        yield return new WaitForSeconds(.5f);        
        m_Rigidbody2D.velocity = new Vector2(0, 0);
        transform.position = basePos;
    }
}