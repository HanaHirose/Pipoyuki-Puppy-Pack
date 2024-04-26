using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerInputAction playerInputAction;

    [SerializeField] private Transform cam;
    [SerializeField] private Animator anim;

    private float speed = 2.0f;
    private float rotationSpeed = 10.0f;

    private Vector3 inputDirection;
    public Vector3 moveDirection;
    private GameObject selectedPuppy;
    private Vector2 rightStickInput = Vector2.zero;
    private float stickThreshold = 0.5f;
    private float prepareAttackTime;
    private float prepareAttackTimeThreshold = 0.3f;



    private enum MovementState { idle, run}

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        playerInputAction = new PlayerInputAction();
        playerInputAction.Player.Enable();
        playerInputAction.Player.Attack.started += AttackStarted;
        playerInputAction.Player.Attack.performed += AttackPerformed;
        playerInputAction.Player.Attack.canceled += AttackCanceled;
        
    }

    private void OnDisable()
    {
        playerInputAction.Player.Attack.performed -= AttackPerformed;
        playerInputAction.Player.Attack.canceled -= AttackCanceled;
        playerInputAction.Player.Disable();

    }


    private void Update()
    {
        float inputHorizontal = playerInputAction.Player.Movement.ReadValue<Vector2>().x;
        float inputVertical = playerInputAction.Player.Movement.ReadValue<Vector2>().y;

        inputDirection = new Vector3(inputHorizontal, 0, inputVertical).normalized;


        //�X�e�B�b�N���͂������ă��[���h���W�n�ł�moveDirection�̌���
        if (inputDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;

            //�ړ������i���[���h���W�Łj
            moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        transform.position += new Vector3(moveDirection.x * speed * Time.deltaTime, 0f, moveDirection.z * speed * Time.deltaTime);

        //�i�s�����������悤�ɂ���
        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

        //CloseFollow();

        UpdateAnimation();

    }

    //�A�j���[�V�������X�V����
    private void UpdateAnimation()
    {
        MovementState state;

        //�ړ����Ȃ�
        if (inputDirection.magnitude >= 0.1f)
        {
            state = MovementState.run;
        }
        else
        {
            state = MovementState.idle;
        }

        anim.SetInteger("state", (int)state);

    }

    ////�q���Ƀv���C���[���߂Â�����Follow��Ԃɂ���
    //private void CloseFollow()
    //{
    //    RaycastHit hit;

    //    // �v���C���[�̑O���Ɉ�苗����Ray���΂�
    //    if (Physics.Raycast(transform.position, transform.forward, out hit, 10f))
    //    {
    //        Debug.Log("Ray hit!");

    //        InuMovement inuMovement = hit.transform.GetComponent<InuMovement>();

    //        //�t���[�̎q���ɐG�����炻�̎q����Following��Ԃɂ���
    //        if (inuMovement != null && inuMovement.GetPuppyState() == InuMovement.PuppyState.Free)
    //        {
    //            inuMovement.SetPuppyState(InuMovement.PuppyState.Following);
    //        }
    //    }
    //}

    //�q���Ƀv���C���[���G������Follow��Ԃɂ���
    //�R���W�������q���ƃv���C���[�̊ԂŊ������ԂłȂ��Ǝg���Ȃ�
    private void OnCollisionEnter(Collision collision)
    {
        //�t���[�̎q���ɐG�����炻�̎q����Following��Ԃɂ���
        InuMovement inuMovement = collision.gameObject.GetComponent<InuMovement>();
        if(inuMovement != null && inuMovement.GetPuppyState() == InuMovement.PuppyState.Free)
        {
            inuMovement.SetPuppyState(InuMovement.PuppyState.Following);
        }
    }


    //�E�X�e�B�b�N���͂��͂��߂̍ŏ��̈�񂾂���΂��B������FindClosestPuppy()���s���B
    private void AttackStarted(InputAction.CallbackContext context)
    {
        selectedPuppy = FindClosestPuppy();

    }

    //�E�X�e�B�b�N���͂Ŏq����Launch�����Ԑ��ɓ���
    private void AttackPerformed(InputAction.CallbackContext context)
    {
        rightStickInput = context.ReadValue<Vector2>();

        //selectedPuppy = FindClosestPuppy();

        if (selectedPuppy != null)
        {
            InuMovement inuMovement = selectedPuppy.GetComponent<InuMovement>(); //�q���̃X�N���v�g���擾
            if(inuMovement != null)
            {
                if(rightStickInput.magnitude > stickThreshold)
                {
                    prepareAttackTime = Time.time; //���݂̎��Ԃ̋L�^���X�V����
                    //inuMovement.SetAttackStartedBool(true);
                    inuMovement.SetPuppyState(InuMovement.PuppyState.AttackPreparing); //�q���̏�Ԃ�AttackPreparing�ɐݒ�
                    //Debug.Log("preparing attack");


                }
            }
            
        }
    }


    //�f�����E�X�e�B�b�N���̓L�����Z���Ŏq����Launch���s��
    private void AttackCanceled(InputAction.CallbackContext context) 
    {
        if (selectedPuppy != null)
        {
            InuMovement inuMovement = selectedPuppy.GetComponent<InuMovement>(); //�q���̃X�N���v�g���擾
            if (inuMovement != null)
            {
                if (inuMovement.GetPuppyState() == InuMovement.PuppyState.AttackPreparing)�@//���߂܂�AttackPreparing��Ԃ������i�X�e�B�b�N�����S�ɖ߂����j
                {
                    if(Time.time - prepareAttackTime > prepareAttackTimeThreshold) // �X�e�B�b�N���������߂���
                    {
                        Debug.Log("Launch Canceled"); //���̔�яo���͒��~����
                        //inuMovement.SetAttackStartedBool(false);
                        inuMovement.SetPuppyState(InuMovement.PuppyState.Following);//�q����Folloing��Ԃɖ߂�
                    }
                    else //�X�e�B�b�N���f�����߂���
                    {
                        //Debug.Log("Launch Puppy!"); //�����яo������
                        inuMovement.SetPuppyState(InuMovement.PuppyState.LaunchStart);//�q����Launch��Ԃɓ���
                    }
                }
            }
        }
    }


    //�A�^�b�N�Ɏg���֐��B�ł��߂��ɂ���q����T���B
    public GameObject FindClosestPuppy()
    {
        GameObject[] puppies;
        puppies = GameObject.FindGameObjectsWithTag("Inu");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        
        foreach(GameObject puppy in puppies)
        {

            //Debug.Log("puppy colse?: " + puppy);
            //Vector3 diff = puppy.transform.position - transform.position;
            //float currentDistance = diff.sqrMagnitude;
            //if (currentDistance < distance)
            //{
            //    closest = puppy;
            //    distance = currentDistance;
            //}

            InuMovement inuMovement = puppy.GetComponent<InuMovement>();
            //�q����Following��Ԃ̏ꍇ�̂݋߂��ɂ��邩���ׂ�B
            if (inuMovement != null && inuMovement.GetPuppyState() == InuMovement.PuppyState.Following)
            {
                Vector3 diff = puppy.transform.position - transform.position;
                float currentDistance = diff.sqrMagnitude;
                if (currentDistance < distance)
                {
                    closest = puppy;
                    distance = currentDistance;
                }
            }
        }
        if(closest == null)
        {
            Debug.Log("No closest puppy in Following state");

        }


        return closest;
    }



    //���̃X�N���v�g����E�X�e�B�b�N�̓��͂ɃA�N�Z�X���邽�߂̊֐�
    public Vector2 GetRightStickInput()
    {
        return rightStickInput;
    }

    //�J�����̌����ɑ΂��ĉE�X�e�B�b�N�̓��͂Ƌt�����̃x�N�g����Ԃ��B
    //�q���̃X�N���v�gInuMovement����Q�Ƃ���p�̊֐��B
    public Vector3 GetPuppyMoveDirection()
    {
        //Debug.Log("rightStick: " + rightStickInput);
        Vector2 reversedRighStickInput = -rightStickInput;
        float targetAngle = Mathf.Atan2(reversedRighStickInput.x, reversedRighStickInput.y) * Mathf.Rad2Deg + cam.eulerAngles.y;

        //�ړ������i���[���h���W�Łj
        Vector3 puppyMoveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

        return puppyMoveDirection;

    }

}
