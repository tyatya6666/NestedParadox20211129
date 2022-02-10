using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using UniRx.Triggers;

namespace NestedParadox.Monsters
{
    public class DustDevil : MonsterBase, IApplyDamage
    {
        [SerializeField] Animator animator;
        [SerializeField] Rigidbody2D rb;
        [SerializeField] SpriteRenderer sprite;
        [SerializeField] Vector3 distanceOffset;
        [SerializeField] float attackSpan;
        [SerializeField] Collider2D attackColl;
        [SerializeField] GameObject attackEffect;
        [SerializeField] GameObject moveEffect;
        [SerializeField] float movingPower;
        [SerializeField] float attackStopDistance;

        private bool canAttack;
        private float attackTime;
        private TempCharacter player;
        private DustDevilState state;
        // Start is called before the first frame update
        void Start()
        {
            canAttack = true;
            attackTime = 0;
            state = DustDevilState.Idle;
            player = GameObject.FindGameObjectWithTag("MainCharacter").GetComponent<TempCharacter>();
            attackColl.OnTriggerEnter2DAsObservable().Subscribe(other => OnAttackHit(other)).AddTo(this);
            Vector3 localScale_temp = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
            Vector3 distanceOffset_temp = new Vector3(distanceOffset.x, distanceOffset.y, distanceOffset.z);
            player.CurrentDirection.Subscribe(x =>
            {
                if(x == 1)
                {
                   // transform.localScale = new Vector3(localScale_temp.x*-1, localScale_temp.y, localScale_temp.z);
                    distanceOffset = new Vector3(distanceOffset_temp.x, distanceOffset.y, distanceOffset.z);
                }
                else if(x == -1)
                {
                    //transform.localScale = new Vector3(localScale_temp.x, localScale_temp.y, localScale_temp.z);
                    distanceOffset = new Vector3(-1, distanceOffset.y, distanceOffset.z);
                }
            });
        }

        void Update()
        {            
            attackTime += Time.deltaTime;                      
            if(attackTime > attackSpan && canAttack)
            {
                attackTime = 0;
                Attack();
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (state == DustDevilState.Idle)//待機中
            {
                transform.position = new Vector3(Mathf.Lerp(transform.position.x, player.transform.position.x - distanceOffset.x, 0.05f),
                                                 Mathf.Lerp(transform.position.y, player.transform.position.y - distanceOffset.y, 0.05f),
                                                 Mathf.Lerp(transform.position.z, player.transform.position.z - distanceOffset.z, 0.05f));
            }
            else if (state == DustDevilState.Attack)//攻撃中
            {

            }
        }

        //ランダムに敵を見つけて連続攻撃
        private async void Attack()
        {
            Debug.Log("Attack開始");
            state = DustDevilState.Attack;
            attackColl.enabled = true;
            GameObject[] targets = GameObject.FindGameObjectsWithTag("Enemy");
            int random = Random.Range(0, targets.Length);
            Vector3 targetPosition = targets[random].transform.position;
            float movingTime = 0;
            List<GameObject> moveEffect_clones = new List<GameObject>();
            for(int i=0; i<4; i++)
            {
                moveEffect_clones.Add(Instantiate(moveEffect, transform.position, Quaternion.identity));
                moveEffect_clones[i].transform.SetParent(transform);
            }            
            sprite.enabled = false;
            animator.SetTrigger("NoneTrigger");
            canAttack = false;

            //攻撃中
            while(movingTime < 4)
            {
                movingTime += Time.deltaTime;
                rb.AddForce((targetPosition - transform.position).normalized * movingPower);
                await UniTask.Yield();
            }

            state = DustDevilState.Idle;
            rb.velocity = Vector3.zero;            
            attackColl.enabled = false;                       
            await UniTask.WaitUntil(() => (transform.position - (player.transform.position - distanceOffset)).magnitude < 0.7f);
            for(int i=0; i<4; i++)
            {
                Destroy(moveEffect_clones[i]);
            }            
            sprite.enabled = true;
            animator.SetTrigger("IdleTrigger");
            canAttack = true;
        }

        private void OnAttackHit(Collider2D other)
        {
            EnemyBase enemy;
            other.TryGetComponent<EnemyBase>(out enemy);
            if(enemy != null)
            {
                enemy.Damaged(attackValue);
                Instantiate(attackEffect, transform.position, Quaternion.identity);
            }            
        }


    }

    public enum DustDevilState
    {
        Idle,
        Attack
    }
}
