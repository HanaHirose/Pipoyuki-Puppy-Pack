using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.InputSystem; // InputAction.CallbackContext context���g�����߂ɕK�v

public class InuMovement : MonoBehaviour
{

    private Rigidbody rb;
    //private PlayerInputAction playerInputAction;
    public Movement movement; //�v���C���[�I�u�W�F�N�g�ɃA�^�b�`���ꂽ�X�N���v�gMovement���Q�Ƃ���i�蓮�Ńt�B�[���h�Ƀh���b�O����K�v����j
    [SerializeField] private Animator anim;


    private GameObject player;
    private Camera cam;

    private float followDistance = 1.0f;
    private float avoidDistance = 0.5f;
    private float speed = 2.0f;
    private float avoidRadius = 0.3f;
    public LayerMask inuMask;
    private float rotationSpeed = 5f;
    private float speedPrepareAttack = 20.0f;
    [SerializeField] private float attackForce = 5.0f; //�q������яo���Ƃ��̗͂̋���
    [SerializeField] private float launchAngle = 65.0f; //Launch���鎞�̊p�x[degree]


    //�A�j���[�V�����p�̏�ԊǗ�
    private enum MovementState 
    { 
        idle, 
        walk,
        attackPreparing,
        launcing,
        landing
    } 
    MovementState state; //�A�j���[�V�����̊Ǘ��Ɏg��

    float moveSpeed;
    //private GameObject selectedPuppy;
    private bool attackStartedBool;

    //�q���̏�Ԃ��Ǘ��B
    public enum PuppyState
    {
        Following,
        AttackPreparing,
        LaunchStart,
        Launching,
        Landing,
        Free
    }
    public PuppyState puppyState = PuppyState.Following; //�ŏ��̎q���̏�Ԃ��w��
    private PuppyState previousPuppyState;

    //private void Awake()
    //{
    //    playerInputAction = new PlayerInputAction();
    //    playerInputAction.Player.Enable();
    //    playerInputAction.Player.Attack.performed += AttackStarted;
    //}


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player");

        GameObject cameraObject = GameObject.FindWithTag("MainCamera");
        if(cameraObject != null)
        {
            cam = cameraObject.GetComponent<Camera>();
        }

        movement = player.GetComponent<Movement>(); //�v���C���[��GameObject����X�N���v�gMovement���擾
    }


    private void LateUpdate()
    {


        //enum���g���Ĕr���I�Ɏq���̏�Ԃ��Ǘ�
        switch(puppyState)
        {
            case PuppyState.Following:
                Follow();
                break;
            case PuppyState.AttackPreparing:
                PuppyAttackPrepare();
                break;
            case PuppyState.LaunchStart:
                PuppyLaunch();
                break;
            case PuppyState.Launching:
                break;
            case PuppyState.Landing:
                break;
            case PuppyState.Free:
                break;
        }



        UpdateAnimation();
    }



    //���[�_�[�ɂ��Ă��������̐���
    private void Follow()
    {
        //�ړ����x���v�Z���邽�߂ɁA�ȑO�̈ʒu���L�^
        Vector3 previousPosition = transform.position;

        Vector3 direction = player.transform.position - transform.position;
        if (direction.magnitude > followDistance)
        {
            direction.Normalize();
            transform.position += direction * speed * Time.deltaTime; //�v���C���[�։���������߂Â�
        }
        else if (direction.magnitude < avoidDistance)
        {
            direction.Normalize();
            transform.position -= direction * speed * Time.deltaTime; //�v���C���[�ɋ߂Â��������痣���
        }


        Vector3 avoidance = Avoidance();

        if (avoidance != Vector3.zero)
        {
            transform.position += avoidance * speed * Time.deltaTime;�@//�����m��������ۂ�
        }



        Quaternion toRotation = Quaternion.LookRotation(player.transform.position - transform.position, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);


        //�ړ����x���v�Z
        moveSpeed = (transform.position - previousPosition).magnitude / Time.deltaTime;

    }




    //�����m��������ۂ��߂̃x�N�g�����v�Z����
    Vector3 Avoidance()
    {
        Collider[] nearInu = Physics.OverlapSphere(transform.position, avoidRadius, inuMask);
        Vector3 avoidance = Vector3.zero;

        foreach(Collider inu in nearInu)
        {
            if(inu.gameObject != gameObject)
            {
                avoidance += (transform.position - inu.transform.position);
            }
        }

        return avoidance;
    }


    //�A�j���[�V�������X�V����
    private void UpdateAnimation()
    {
        

        if(puppyState == PuppyState.Following)
        {
            if (moveSpeed > 0.1f)
            {
                state = MovementState.walk;
            }
            else
            {
                state = MovementState.idle;
            }

        }
        else if(puppyState == PuppyState.AttackPreparing)
        {
            state = MovementState.attackPreparing;
        }
        else if(puppyState == PuppyState.Launching)
        {
            state = MovementState.launcing;
        }
        else if(puppyState == PuppyState.Landing)
        {
            state = MovementState.landing;
            SetPuppyState(PuppyState.Free); //Landing�̃A�j���[�V������Ԃ����s���Ă���q���̏�Ԃ�Free�ɂ���
        }
        else if(puppyState == PuppyState.Free)
        {
            state = MovementState.idle;
        }

        anim.SetInteger("state", (int)state);
    }


    

    //enum�̎q���̏�Ԃ��O���X�N���v�g����ύX���邽�߂̊֐�
    public void SetPuppyState(PuppyState state)
    {
        //�ȑO�̏�Ԃ��L�^
        previousPuppyState = puppyState;
        //��Ԃ�V�������̂ɍX�V
        puppyState = state;
    }

    //enum�̎q���̏�Ԃ��O���X�N���v�g����擾���邽�߂̊֐�
    public PuppyState GetPuppyState()
    {
        return puppyState;
    }


    //�q����attack���鏀���Ԑ��ɓ���
    private void PuppyAttackPrepare()
    {
        //�J�����̌������Q�Ƃ��ăv���C���[�̈ʒu����J�����̌����ɑ΂��č��Ɍ����z�u�����悤�ɂ���B
        Vector3 cameraLeft = Vector3.Cross(cam.transform.up, cam.transform.forward);
        float offset = 0.25f;
        Vector3 targetPosition = player.transform.position - cameraLeft * offset;

        //�q�����v���C���[�̐^���ɗ���悤�ɂ���
        //transform.position = targetPosition;
        transform.position = Vector3.Lerp(transform.position, targetPosition, speedPrepareAttack * Time.deltaTime); //�u�Ԉړ����Ȃ��Ŋ��炩�Ɉړ�����
        //�q�����v���C���[�Ɠ��������������悤�ɂ���
        //transform.rotation = player.transform.rotation;

        //�E�X�e�B�b�N�̓��͌����ɉ����Ďq���̌����𐧌䂷��
        Quaternion toRotation = Quaternion.LookRotation(movement.GetPuppyMoveDirection(), Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);

    }

    //�q����attack����
    private void PuppyLaunch()
    {
        if(previousPuppyState != puppyState)
        {
            //���[���h���W�n�ł�z�����̃x�N�g����xz���ʂ���45�x����������Ɍ�����
            Vector3 upVector = Quaternion.Euler(-launchAngle, 0, 0) * Vector3.forward;
            //��L�̃x�N�g���������y������ɉ�]�����Ďq���������Ă�����ɂ���
            Vector3 rotatedVector = Quaternion.Euler(0, transform.eulerAngles.y, 0) * upVector;

            //�q���ɗ͂������Ĕ�яo������
            rb.AddForce(rotatedVector * attackForce, ForceMode.Impulse);

            //�q���̏�Ԃ�LaunchStarted����Launching�ɐ؂�ւ���
            SetPuppyState(PuppyState.Launching);
            //�v���C���[�Ɗ����Ȃ����C���[�Ɏq�����ꎞ�I�Ɉڂ�
            gameObject.layer = LayerMask.NameToLayer("NoInteract");

        }
    }






    private void OnCollisionEnter(Collision collision)
    {
        //�q����Launcing����Ground�ɒ��n�����Landing��ԂɂȂ鏈��
        if (puppyState == PuppyState.Launching && collision.gameObject.tag == "Ground")
        {
            SetPuppyState(PuppyState.Landing);
            //�q���̃v���C���[�Ɗ����Ȃ����C���[���猳�̃��C���[�ɖ߂�
            gameObject.layer = LayerMask.NameToLayer("InuMask");

        }
    }

    

}
