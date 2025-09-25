using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] 
    private float _speed = 1.0f;
    
    [SerializeField, Tooltip("The minimum distance we will count (ignores small jitters)")]
    private float minMoveEpsilon = 0.001f;
    [SerializeField, Range(0f, 1f), Tooltip("Minimum ratio (actual/expected) to consider movement as progress")]
    private float minProgressRatio = 0.25f;
    
    [SerializeField] 
    private Animator _animator;
    
    [SerializeField]
    private Rigidbody2D _rigidbody2D;
    [SerializeField]
    private SpriteRenderer _spriteRenderer;
    [SerializeField, Tooltip("LayerMask used for walls collisions (set in inspector)")]
    private LayerMask _wallLayerMask;

    private Vector2 _userInput;
    private float _inputEpsilon = 0.01f;
    private float _inputEpsilonSq;
    
    private Vector2 _lastPosition;
    private float _totalDistance;
    private bool _lastIsWalking;
    private bool _lastFlipX;

    public void Init(ref Vector2 startPosition)
    {
        _wallLayerMask = LayerMask.GetMask("Wall");

        _rigidbody2D.position = startPosition;
        _rigidbody2D.linearVelocity = Vector2.zero;
        _lastPosition = _rigidbody2D.position;
        _totalDistance = 0f;
        _inputEpsilonSq = _inputEpsilon * _inputEpsilon;
    }
    
    public int GetTotalDistance()
    {
        return Mathf.RoundToInt(_totalDistance);
    }

    public void UpdateInput()
    {
        _userInput = new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
        AnimateMovement();
    }

    public void Move()
    {
        if (_userInput.sqrMagnitude < _inputEpsilonSq)
        {
            _lastPosition = _rigidbody2D.position;
            return;
        }

        var desiredDelta = _userInput.normalized * _speed * Time.fixedDeltaTime;
        var desiredDistance = desiredDelta.magnitude;

        var desiredPos = _rigidbody2D.position + desiredDelta;
        
        var hit = Physics2D.Raycast(_rigidbody2D.position, desiredDelta.normalized, desiredDelta.magnitude, _wallLayerMask);

        if (hit.collider == null)
        {
            _rigidbody2D.MovePosition(desiredPos);
        }

        var currentPos = _rigidbody2D.position;
        var actualDistance = Vector2.Distance(currentPos, _lastPosition);

        if (actualDistance < minMoveEpsilon)
        {
            _lastPosition = currentPos;
            return;
        }

        if (desiredDistance > Mathf.Epsilon)
        {
            var ratio = actualDistance / desiredDistance;
            if (ratio < minProgressRatio)
            {
                _lastPosition = currentPos;
                return;
            }
        }

        _totalDistance += actualDistance;
        _lastPosition = currentPos;
    }

    private void AnimateMovement()
    {
        var isMoving = _userInput.sqrMagnitude > 0f;

        if (_lastIsWalking != isMoving)
        {
            _animator.SetBool("walk", isMoving);
            _lastIsWalking = isMoving;
        }

        if (isMoving)
        {
            var absX = Mathf.Abs(_userInput.x);
            var absY = Mathf.Abs(_userInput.y);
            var hasHorizontalIntent = absX > 0.001f && absX >= absY;
            if (hasHorizontalIntent)
            {
                var desiredFlip = _userInput.x < 0f;
                if (_lastFlipX != desiredFlip)
                {
                    _spriteRenderer.flipX = desiredFlip;
                    _lastFlipX = desiredFlip;
                }
            }
        }
    }

    public void StopMovement()
    {
        _userInput = Vector2.zero;
        _rigidbody2D.linearVelocity = Vector2.zero;
        _rigidbody2D.angularVelocity = 0f;
        if (_lastIsWalking)
        {
            _animator.SetBool("walk", false);
            _lastIsWalking = false;
        }
    }
}
