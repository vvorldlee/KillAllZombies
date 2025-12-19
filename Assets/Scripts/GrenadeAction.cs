using UnityEngine;

public class GrenadeAction : MonoBehaviour
{
    public GameObject GrenadeEffect;
    public int grenadeDamage = 15;
    public float explosionRadius = 5f;

    private void OnCollisionEnter(Collision collision)
    {
        GameObject gef = Instantiate(GrenadeEffect);
        gef.transform.position = transform.position;

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hitCollider in colliders)
        {
            EnemyFSM enemyFSM = hitCollider.GetComponent<EnemyFSM>();
            if (enemyFSM != null)
            {
                enemyFSM.HitEnemy(grenadeDamage, transform.position);
            }
        }

        Destroy(gameObject);
    }
}