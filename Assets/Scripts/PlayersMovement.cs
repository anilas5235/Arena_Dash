using UnityEngine;
using UnityEngine.UI;

public class PlayersMovement : MonoBehaviour
{
    [SerializeField] private bool yinOrYang; //true = Yin ;false = Yang
    [SerializeField] private float speedModifier, maxSpeedDelta;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Image[] uiHP;
    [SerializeField] private Sprite fullHard, emptyHard;

    private Rigidbody2D _rigidbody2D;
    private Vector2 _desiredVelocity;
    private int _currentHP, _maxHP;
    
    // Start is called before the first frame update
    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _maxHP = _currentHP = uiHP.Length;
        UIHardUpdate();
    }

    // Update is called once per frame
    private void Update()
    {

        if (yinOrYang)
        {
            _desiredVelocity = new Vector2(Input.GetAxisRaw("YinHorizontal"), Input.GetAxisRaw("YinVertical")).normalized * speedModifier;
        }
        else
        {
            _desiredVelocity = new Vector2(Input.GetAxisRaw("YangHorizontal"), Input.GetAxisRaw("YangVertical")).normalized * speedModifier;
        }
    }

    private void FixedUpdate()
    {
        _rigidbody2D.velocity = Vector2.MoveTowards(_rigidbody2D.velocity, _desiredVelocity, maxSpeedDelta); //look up how moveDelta works here
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        switch (col.transform.tag)
        {
            case "Player": break;
            case "DeathZone": _currentHP--; transform.position = respawnPoint.position; UIHardUpdate(); break;
        }
    }

    private void UIHardUpdate()
    {
        for (int i = 0; i < uiHP.Length; i++)
        {
            uiHP[i].sprite = _currentHP > i ? fullHard : emptyHard; print(_currentHP);
        }
    }
}
