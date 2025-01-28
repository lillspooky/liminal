using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class ArcherController : MonoBehaviour
    {
        public float Range;
        public string Name;
        public bool isEnemy = false;
        public bool isPatroller = false;
        public float speed = 0;
        public int health = 100;
        public int damagePower = 5;
        public float attackPeriod = 1.5f;
        private float lastAttackTime = 0;
        private float lastCheckPeriod = 0.5f;
        private float lastCheck = 0;
        public NavMeshAgent agent;
        public Animator anim;
        private int TotalHeath = 100;
        private Transform target;
        public Transform[] Patrolling_Points;
        public Status status;
        public AudioClip[] Audio_Attacks;
        public AudioClip[] Audio_Realize;
        public AudioClip[] Audio_Die;
        public AudioSource AudioSource;
        public GameObject arrowPrefab;
        public Transform arrowStartPoint;
        private float lastCheckForRealize = 0;
        public GainScript gain;
        float dyingYpos = 0;

        private void OnEnable()
        {
            TotalHeath = health;
            attackPeriod = Random.Range(attackPeriod - 0.25f, attackPeriod + 0.25f);
            lastCheckPeriod = Random.Range(lastCheckPeriod, attackPeriod + 0.2f);
            Idle();
        }

        public void Idle()
        {
            target = null;
            if(status != Status.Idle)
            {
                isSent = false;
                if(arrow != null)
                {
                    Destroy(arrow);
                }
                anim.SetTrigger("ReturnToIdle");
                anim.ResetTrigger("AttackReady");
                anim.ResetTrigger("Realize1");
                anim.ResetTrigger("Realize2");
                anim.ResetTrigger("Attack");
            }

            status = Status.Idle;
            float decisionTime = Random.Range(0, 6);
            StartCoroutine(DecideWhattoDo(decisionTime));
        }

        public void Realize()
        {
            if (status == Status.Realizing) return;
            if (agent != null)
            {
                agent.isStopped = true;
            }
            anim.SetTrigger("Realize" + Random.Range(1, 3).ToString());
            status = Status.Realizing;
            if (Random.Range(0, 2) == 0)
            {
                StartCoroutine(PlaySound(SoundType.Realize));
            }
            StartCoroutine(HitStatusAfterRealize());
        }

        IEnumerator HitStatusAfterRealize()
        {
            yield return new WaitForSeconds(1.5f);
            target = HeroController.instance.transform;
            transform.LookAt(target);
            status = Status.Hitting;
        }

        IEnumerator DecideWhattoDo(float time)
        {
            yield return new WaitForSeconds(time);
            if (status == Status.Idle)
            {
                int decision = isPatroller ? Random.Range(0, 2) : 0;
                if (decision == 0)
                {
                    Idle();
                }
                else if (decision == 1)
                {
                    StartCoroutine(GoSomeWhere(false));
                }
            }
        }

        public IEnumerator GoSomeWhere(bool Escape)
        {
            if (agent != null)
            {
                if (Patrolling_Points != null && Patrolling_Points.Length > 0)
                {
                    target = Patrolling_Points[Random.Range(0, Patrolling_Points.Length)];
                    agent.angularSpeed = 0;
                    agent.ResetPath();
                    agent.autoRepath = true;
                    agent.SetDestination(target.position);
                    yield return new WaitUntil(() => !agent.pathPending);
                    agent.angularSpeed = 120;
                    int moveType = Escape ? 0 : 1;
                    if (moveType == 0)
                    {
                        status = Escape ? Status.Escaping : Status.Moving;
                        agent.speed = speed * 2;
                    }
                    else
                    {
                        status = Status.Moving;
                        agent.speed = speed;
                    }
                }
            }
        }

        void Update()
        {
            if (status == Status.Died || gettingHit)
            {
                if (status == Status.Died && transform.position.y > dyingYpos)
                {
                    transform.position = new Vector3(transform.position.x, dyingYpos, transform.position.z);
                }
                return;
            }
            else if (status == Status.Escaping || status == Status.Moving)
            {
                if (target != null && Time.time > lastCheck + lastCheckPeriod && Vector3.Distance(transform.position, target.position) > Range)
                {
                    lastCheck = Time.time;
                    if (agent != null)
                    {
                        agent.isStopped = true;
                    }
                    Idle();
                }
            }
            else if (status == Status.Hitting)
            {
                Attack();
            }
            if (agent != null && agent.enabled)
            {
                anim.SetFloat("locomotion", agent.velocity.magnitude);
            }
            CheckForRealize();
            if (target != null)
            {
                target = HeroController.instance.transform;
            }
            if (gettingReadyForAttack && arrow != null)
            {
                arrow.transform.rotation = transform.rotation;
            }

        }

        private bool gettingReadyForAttack = false;
        private void CheckForRealize()
        {
            if (Time.time > lastCheckForRealize + 1)
            {
                lastCheckForRealize = Time.time;
                if (Vector3.Distance(transform.position, HeroController.instance.transform.position) <= Range && !HeroController.instance.isInvisible && HeroController.instance.Health > 0)
                {
                    if (status == Status.Idle || status == Status.Moving)
                    {
                        Realize();
                    }
                }
                else
                {
                    if (agent != null)
                    {
                        agent.isStopped = true;
                    }
                    Idle();
                }
            }
        }

        public void TargetYourRival()
        {
            if (status == Status.Escaping || status == Status.Moving)
            {
                if (agent != null)
                {
                    agent.isStopped = true;
                }
            }
            target = HeroController.instance.transform;
            status = Status.Hitting;
        }

        public void GetDamage(int damage)
        {
            if (health <= 0)
            {
                return;
            }

            health = health - damage;
            GameCanvas_Controller.instance.Show_EnemyHealthPanel(Name, health, TotalHeath);
            if (health > 0)
            {
                TargetYourRival();
            }
            else
            {
                if (agent != null)
                {
                    agent.isStopped = true;
                }
                health = 0;
                dyingYpos = transform.position.y;
                status = Status.Died;
                if (agent != null)
                {
                    agent.enabled = false;
                }
                // Rigidbody'i aktiflestirelim ki düsebilsin!
                Rigidbody rigidbody = transform.gameObject.AddComponent<Rigidbody>();
                rigidbody.mass = 1000;
                rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                StartCoroutine(DeactivateAnimator());
                StartCoroutine(PlaySound(SoundType.Die));
                if (gain != null)
                {
                    gain.Spread();
                }
                Destroy(gameObject, 5);
            }
            anim.SetInteger("Health", health);
            anim.SetTrigger("GetHit");
            StartCoroutine(ResetGetHit());
        }

        IEnumerator DeactivateAnimator()
        {
            yield return new WaitForSeconds(2.5f);
            anim.enabled = false;
        }

        private bool gettingHit = false;

        IEnumerator ResetGetHit()
        {
            gettingHit = true;
            yield return new WaitForSeconds(0.75f);
            gettingHit = false;
        }

        private float lastTimeSend = 0;

        public void GoToEnemy()
        {
            if (agent != null && agent.enabled)
            {
                if (Time.time > lastTimeSend + 0.5f)
                {
                    lastTimeSend = Time.time;
                    agent.ResetPath();
                    agent.SetDestination(target.position);
                    agent.speed = speed * 2;
                }
            }
        }

        public void Attack()
        {
            Vector3 direction = target.position - transform.position;
            direction.y = 0;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction), 5 * Time.deltaTime);
            if (Time.time >= lastAttackTime + attackPeriod)
            {
                lastAttackTime = Time.time;
                if (target != null)
                {
                    float distance = Vector3.Distance(transform.position, target.position);
                    if (distance > 100f)
                    {
                        GoToEnemy();
                    }
                    else if (distance <= 100f)
                    {
                        if (agent != null)
                        {
                            agent.isStopped = true;
                        }
                        HitNow();
                    }
                }
            }
        }

        public IEnumerator ShootNow(GameObject arrow)
        {
            yield return new WaitForSeconds(2f);
            if (target != null)
            {
                anim.SetTrigger("Attack");
                yield return new WaitForSeconds(0.2f);
                if(arrow != null)
                {
                    Rigidbody rigidbody = arrow.GetComponent<Rigidbody>();
                    rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                    rigidbody.isKinematic = false;
                    rigidbody.useGravity = true;
                    isSent = false;
                    if (target != null)
                    {
                        Vector3 targettoShoot2 = new Vector3(target.transform.position.x + Random.Range(-1.5f, 1.5f), target.transform.position.y + Random.Range(0f, 3.5f), target.transform.position.z + Random.Range(-1.5f, 1.5f));
                        arrow.transform.LookAt(targettoShoot2);
                    }
                    arrow.transform.rotation = transform.rotation;
                    rigidbody.AddForce((arrow.transform.forward + Vector3.up / 7) * 20, ForceMode.Impulse);
                    arrow.GetComponent<ArrowScript>().Shooted(damagePower);
                    arrow.transform.parent = null;
                }
                gettingReadyForAttack = false;
                StartCoroutine(PlaySound(SoundType.Hit));
            }
        }

        private bool isSent = false;
        GameObject arrow;
        void HitNow()
        {
            if (Random.Range(0, 4) < 3 && !gettingHit)
            {
                if (!isSent)
                {
                    isSent = true;
                    arrow = Instantiate(arrowPrefab, arrowStartPoint.transform);
                    arrow.transform.localPosition = Vector3.zero;
                    gettingReadyForAttack = true;
                    arrow.transform.rotation = transform.rotation;
                    anim.SetTrigger("AttackReady");
                    StartCoroutine(ShootNow(arrow));
                    if (HeroController.instance.Health <= 0)
                    {
                        Idle();
                    }
                }
            }
        }

        IEnumerator PlaySound(SoundType type)
        {
            yield return new WaitForSeconds(0.5f);
            switch (type)
            {
                case SoundType.Die:
                    AudioSource.clip = Audio_Die[Random.Range(0, Audio_Die.Length)];
                    break;
                case SoundType.Hit:
                    AudioSource.clip = Audio_Attacks[Random.Range(0, Audio_Attacks.Length)];
                    break;
                case SoundType.Realize:
                    AudioSource.clip = Audio_Realize[Random.Range(0, Audio_Realize.Length)];
                    break;
            }
            AudioSource.Play();
        }
    }

    public enum SoundType
    {
        Die,
        Hit,
        Idle,
        Realize
    }
}