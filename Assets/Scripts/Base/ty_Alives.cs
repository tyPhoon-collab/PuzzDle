using UnityEngine;

public abstract class ty_Alives : MonoBehaviour
{
    [SerializeField] protected int hp;
    [SerializeField] protected int hpMax;
    [SerializeField] protected int atk;
    [SerializeField] protected int def;

    protected GameObject tyGameObject;
    protected ty_Log tyLog;
    protected Color damageColor = Color.white;
    protected Color recoverColor = Color.green;
    protected bool isDrawRecover = false;

    protected virtual void Awake() {
        tyGameObject = GameObject.FindGameObjectWithTag("GameController");
        tyLog = tyGameObject.GetComponent<ty_Log>();
    }


    public int Hp {
        get { return hp; }
        set {
            int diff = value - hp;
            hp = Mathf.Clamp(value, 0, hpMax);

            if (diff < 0) 
                tyLog.DrawDamage(-diff, transform.position, damageColor);
            else if (isDrawRecover && diff > 0) 
                tyLog.DrawRecover(diff, transform.position, recoverColor);
            
            if (diff != 0) OnHpChanged();
            if (hp == 0) Dead(); 
        }
    }

    public int HpMax {
        get { return hpMax; }
        set {
            hpMax = Mathf.Clamp(value, 0, value);
            OnHpMaxChanged();
        }
    }

    public int Atk {
        get { return atk; }
        set {
            atk = Mathf.Clamp(value, 0, value);
            OnAtkChanged();
        }
    }

    public int Def {
        get { return def; }
        set {
            def = Mathf.Clamp(value, 0, value);
            OnDefChanged();
        }
    }

    protected virtual void OnHpChanged() { }
    protected virtual void OnHpMaxChanged() { }
    protected virtual void OnAtkChanged() { }
    protected virtual void OnDefChanged() { }
    protected abstract void Dead();

}
