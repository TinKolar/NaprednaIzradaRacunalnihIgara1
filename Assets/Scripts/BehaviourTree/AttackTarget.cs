using System.Collections;
using UnityEngine;
using TheKiwiCoder;

public class AttackTarget : ActionNode
{
    public float AttackRate = 2f;
    public float AttackRange = 1.5f;
    public int Damage = 20;
    public float HitDelay = 1f; // koliko nakon pocetka animacije udarac pogodi

    private Enemy _targetEnemy;
    private PlayerStats _targetStats;
    private float _nextAttack;
    private Coroutine _attacking;

    protected override void OnStart()
    {
        _targetEnemy = context.Target != null ? context.Target.GetComponent<Enemy>() : null;
        _targetStats = context.Target != null ? context.Target.GetComponent<PlayerStats>() : null;
    }

    protected override void OnStop()
    {
    }

    protected override State OnUpdate()
    {
        if (context.Target == null ||
            !context.Target.gameObject.activeSelf ||
            (context.Target.position - context.transform.position).magnitude > AttackRange ||
            TargetHealth() <= 0)
        {
            return State.Failure;
        }

        Vector3 dir = (context.Target.position - context.transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            context.transform.rotation = Quaternion.Lerp(context.transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
        }

        if (Time.time >= _nextAttack)
        {
            _nextAttack = Time.time + AttackRate;
            context.Enemy.PlayAttack();

            //korutina umjesto DOVirtual.DelayedCall (node nije MonoBehaviour pa je pokrece Enemy)
            if (_attacking != null)
            {
                context.Enemy.StopCoroutine(_attacking);
            }
            _attacking = context.Enemy.StartCoroutine(DealDamageAfterDelay());
        }

        return State.Running;
    }

    private IEnumerator DealDamageAfterDelay()
    {
        yield return new WaitForSeconds(HitDelay);

        if (context.Target != null && (context.Target.position - context.transform.position).magnitude <= AttackRange)
        {
            if (_targetEnemy != null)
            {
                _targetEnemy.TakeDamage(Damage);
            }
            else if (_targetStats != null)
            {
                //armour smanjuje stetu (pola armoura), uvijek barem 1
                int taken = Mathf.Max(1, Damage - Mathf.RoundToInt(_targetStats.Armour.Value) / 2);
                _targetStats.Health.Decrease(taken); //UI se osvjezi sam preko eventa
                DamageNumber.Spawn(context.Target.position + Vector3.up * 2.1f, taken, new Color(1f, 0.18f, 0.12f));
                SFX.PlayAt(SFX.Instance.PlayerHurt, context.Target.position);
                Debug.Log(context.gameObject.name + " hit the player for " + taken);
            }
        }
        _attacking = null;
    }

    private float TargetHealth()
    {
        if (_targetEnemy != null) return _targetEnemy.Health;
        if (_targetStats != null) return _targetStats.Health.Current;
        return 1;
    }
}
