using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public bool moveRight;
    public float speed;
    public BoxCollider2D turnPointCollider;
    public Rigidbody2D m_Rigidbody2D;
    public PlayerController player;

    private void Awake()
    {
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void FixedUpdate()
    {
        if (moveRight)
        {
            m_Rigidbody2D.velocity = new Vector2(Time.deltaTime * speed, m_Rigidbody2D.velocity.y);
            //transform.Translate(Time.deltaTime * speed, 0, 0);
            transform.localScale = new Vector2(1, 1);
        }
        else
        {
            m_Rigidbody2D.velocity = new Vector2(-Time.deltaTime * speed, m_Rigidbody2D.velocity.y);
            //transform.Translate(-Time.deltaTime * speed, 0, 0);
            transform.localScale = new Vector2(-1, 1);
        }
        //if (m_Rigidbody2D.velocity.x < 0) moveRight=false;
        //else moveRight=true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (moveRight) moveRight = false;
        else if (!moveRight) moveRight = true;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StartCoroutine(player.GotHit(.5f, 10, player.transform.position, transform.position.x));
            //StartCoroutine(GotHit(.05f, 1, transform.position, player.transform.position.x));
        }
    }

    public IEnumerator GotHit(float knockDur, float knockPwr, Vector2 knockDir, float enemyX)
    {
        float launchDir = 0;
        float launchMagnitude = 2f;
        if (enemyX > knockDir.x)
        {
            launchDir = -launchMagnitude;
            moveRight = true;
        }
        else if (enemyX < knockDir.x)
        {
            launchDir = launchMagnitude;
            moveRight = false;
        }
        float timer = 0;
        while (knockDur > timer)
        {
            timer += Time.deltaTime;
            m_Rigidbody2D.AddForce(new Vector3(knockDir.x + launchDir, knockPwr, transform.position.z));
        }
        yield return 0;
    }
}