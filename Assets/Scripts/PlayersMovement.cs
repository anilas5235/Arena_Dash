using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayersMovement : MonoBehaviour
{
    [SerializeField] private bool yinOrYang; //true = Yin ;false = Yang
    [SerializeField] private float speedModifier, maxSpeedDelta, dashSpeedMultiplier;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Image[] uiHP;
    [FormerlySerializedAs("fullHard")] [SerializeField] private Sprite fullHeart;
    [FormerlySerializedAs("emptyHard")] [SerializeField] private Sprite emptyHeart;
    [SerializeField] private Animator animator;

    private Rigidbody2D _rigidbody2D;
    private Vector2 _desiredVelocity,_oldVeclocity;
    private int _currentHP, _maxHP;
    private float _currentDashCharge,_maxDashCharge = 2f ;
    private bool _canChargeDash = true, stuned;
    
    // Start is called before the first frame update
    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        _maxHP = _currentHP = uiHP.Length;
        UIHeartUpdate();
    }

    // Update is called once per frame
    private void Update()
    {

        if (yinOrYang)
        {
            if (!_canChargeDash || stuned ) { return; }
            _desiredVelocity = new Vector2(Input.GetAxisRaw("YinHorizontal"), Input.GetAxisRaw("YinVertical")).normalized * speedModifier;
            if (Input.GetButton("Jump"))
            {
                if (_currentDashCharge < _maxDashCharge && _canChargeDash)
                {
                    _currentDashCharge += Time.deltaTime;
                    animator.SetInteger("DashState",1);
                }
            }

            if (Input.GetButtonUp("Jump"))
            {
                _canChargeDash = false;
                if (_currentDashCharge > 1f)
                { StartCoroutine( Dash()); }
                else
                {
                    _canChargeDash = true; 
                    animator.SetInteger("DashState",0);
                }
                _currentDashCharge = 0;
            }
        }
        else
        {
            _desiredVelocity = new Vector2(Input.GetAxisRaw("YangHorizontal"), Input.GetAxisRaw("YangVertical")).normalized * speedModifier;
            if (Input.GetKey(KeyCode.RightControl))
            {
                if (_currentDashCharge < _maxDashCharge && _canChargeDash)
                {
                    _currentDashCharge += Time.deltaTime;
                    animator.SetInteger("DashState",1);
                }
            }

            if (Input.GetKeyUp(KeyCode.RightControl))
            {
                _canChargeDash = false;
                if (_currentDashCharge > 1f)
                { StartCoroutine( Dash()); }
                else
                {
                    _canChargeDash = true;
                    animator.SetInteger("DashState",0);
                }
                _currentDashCharge = 0;
            }
        }

        if (_rigidbody2D.velocity != _oldVeclocity)
        {
            if (_rigidbody2D.velocity.magnitude <0.1f)
            {
                transform.localRotation = Quaternion.Euler(new Vector3(0, 0,
                    Mathf.Atan2(_oldVeclocity.x, -_oldVeclocity.y) * Mathf.Rad2Deg % 360));
            }
            else
            {
                transform.localRotation = Quaternion.Euler(new Vector3(0, 0,
                                Mathf.Atan2(_rigidbody2D.velocity.x, -_rigidbody2D.velocity.y) * Mathf.Rad2Deg % 360));
                _oldVeclocity = _rigidbody2D.velocity;
            }
        }
    }

    private void FixedUpdate()
    {
        if (stuned) { return; }
        _rigidbody2D.velocity = Vector2.MoveTowards(_rigidbody2D.velocity, _desiredVelocity, maxSpeedDelta); //look up how moveDelta works here
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        switch (col.transform.tag)
        {
            case "Player":
                if (_canChargeDash) {return; } 
                col.gameObject.GetComponent<PlayersMovement>().StartCoroutine( Stun(2f));
                col.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                col.gameObject.GetComponent<Rigidbody2D>().velocity = _rigidbody2D.velocity * 1.5f;
                _rigidbody2D.velocity = Vector2.zero;  StartCoroutine(Stun(0.5f)); _desiredVelocity = Vector2.zero;
                _canChargeDash = true;  break;
            case "DeathZone": _currentHP--; transform.position = respawnPoint.position; UIHeartUpdate(); break;
        }
    }

    private void UIHeartUpdate()
    {
        if (_currentHP > _maxHP) { _currentHP = _maxHP; }
        for (int i = 0; i < uiHP.Length; i++)
        {
            uiHP[i].sprite = _currentHP > i ? fullHeart : emptyHeart;
        }
    }

    private IEnumerator Dash()
    {
        if(_desiredVelocity.magnitude<0.1f){ animator.SetInteger("DashState",0); }
        else
        {
            animator.SetInteger("DashState", 2);
            _rigidbody2D.velocity = _rigidbody2D.velocity.normalized * (speedModifier * dashSpeedMultiplier * (_currentDashCharge / _maxDashCharge));
            maxSpeedDelta = 0.1f;
            yield return new WaitForSeconds(7 / _desiredVelocity.magnitude);
            _canChargeDash = true;
            maxSpeedDelta = 0.4f;
            animator.SetInteger("DashState", 0);
        }
    }

    public IEnumerator Stun(float stunTime)
    {
        stuned = true;
        yield return new WaitForSeconds(stunTime);
        stuned = false;
    }
}
