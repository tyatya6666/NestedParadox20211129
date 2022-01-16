using UniRx;
using UnityEngine;

namespace NestedParadox.Players
{
    public class PlayerMove : MonoBehaviour
    {
        // 接地状態
        public IReadOnlyReactiveProperty<bool> IsGrounded => _isGrounded;
        // 落下中であるか
        // public bool IsFall => _rigidbody2D.velocity.y < 0;


        // 移動速度
        [SerializeField] private float _dashSpeed = 3;
        // ジャンプ速度
        [SerializeField] private float _jumpSpeed = 5.5f;


        //行動不能
        private bool _isMoveBlock;
        //地面の判定に使うレイ
        private readonly RaycastHit2D[] _raycastHitResults = new RaycastHit2D[1];
        //地面の判定
        private readonly ReactiveProperty<bool> _isGrounded = new BoolReactiveProperty(true);


        //外部参照
        private PlayerCore _playerCore;
        private Rigidbody2D _rigidbody2D;
        private PlayerInput _playerinput;

        private void Start(){
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _playerCore = GetComponent<PlayerCore>();
            _playerinput = GetComponent<PlayerInput>();
        }

        private void FixedUpdate()
        {
            // 接地判定処理
            CheckGrounded();

            // 上書きする移動速度の値
            var vel = Vector3.zero;

            // 操作イベントから得られた移動量
            var moveVector = GetMoveVector();

            // 移動操作を反映する
            if (moveVector != Vector3.zero)
            {
                Debug.Log("移動");
                vel = moveVector * _dashSpeed;
            }

            // ジャンプ
            if (_playerinput.IsJump.Value)
            {
                Debug.Log("Jump");
                vel += Vector3.up * _jumpSpeed;
            }

            // 重力落下分を維持する
            vel += new Vector3(0, _rigidbody2D.velocity.y, 0);

            // 速度を更新
            _rigidbody2D.velocity = vel;
        }

        // 操作イベントの値から移動量を決定する
        private Vector3 GetMoveVector()
        {
            var x = _playerinput.MoveDirection.Value.x;
            if (x > 0.1f)
            {
                //ここで向き変更の通知をカメラに送る
                Debug.Log("右");
                return Vector3.right;
            }
            else if (x < -0.1f)
            {
                return -Vector3.right;
            }
            else
            {
                return Vector3.zero;
            }
        }

        // 接地判定
        private void CheckGrounded()
        {
            // 地面に対してRaycastを飛ばして接地判定を行う
            // var hitCount = Physics2D.CircleCastNonAlloc(
            //     origin: transform.position - new Vector3(0, _characterHeightOffset, 0),
            //     radius: 0.09f,
            //     direction: Vector2.down,
            //     results: _raycastHitResults,
            //     distance: 0.01f,
            //     layerMask: _groundMask);

            // _isGrounded.Value = hitCount != 0;
        }

        // 移動不可フラグ
        public void BlockMove(bool isBlock)
        {
            _isMoveBlock = isBlock;
        }
    }
}