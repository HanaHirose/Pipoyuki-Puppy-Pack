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


        //スティック入力をうけてワールド座標系でのmoveDirectionの決定
        if (inputDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;

            //移動方向（ワールド座標で）
            moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        transform.position += new Vector3(moveDirection.x * speed * Time.deltaTime, 0f, moveDirection.z * speed * Time.deltaTime);

        //進行方向を向くようにする
        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

        //CloseFollow();

        UpdateAnimation();

    }

    //アニメーションを更新する
    private void UpdateAnimation()
    {
        MovementState state;

        //移動中なら
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

    ////子犬にプレイヤーが近づいたらFollow状態にする
    //private void CloseFollow()
    //{
    //    RaycastHit hit;

    //    // プレイヤーの前方に一定距離のRayを飛ばす
    //    if (Physics.Raycast(transform.position, transform.forward, out hit, 10f))
    //    {
    //        Debug.Log("Ray hit!");

    //        InuMovement inuMovement = hit.transform.GetComponent<InuMovement>();

    //        //フリーの子犬に触ったらその子犬をFollowing状態にする
    //        if (inuMovement != null && inuMovement.GetPuppyState() == InuMovement.PuppyState.Free)
    //        {
    //            inuMovement.SetPuppyState(InuMovement.PuppyState.Following);
    //        }
    //    }
    //}

    //子犬にプレイヤーが触ったらFollow状態にする
    //コリジョンが子犬とプレイヤーの間で干渉する状態でないと使えない
    private void OnCollisionEnter(Collision collision)
    {
        //フリーの子犬に触ったらその子犬をFollowing状態にする
        InuMovement inuMovement = collision.gameObject.GetComponent<InuMovement>();
        if(inuMovement != null && inuMovement.GetPuppyState() == InuMovement.PuppyState.Free)
        {
            inuMovement.SetPuppyState(InuMovement.PuppyState.Following);
        }
    }


    //右スティック入力しはじめの最初の一回だけよばれる。そこでFindClosestPuppy()を行う。
    private void AttackStarted(InputAction.CallbackContext context)
    {
        selectedPuppy = FindClosestPuppy();

    }

    //右スティック入力で子犬のLaunch準備態勢に入る
    private void AttackPerformed(InputAction.CallbackContext context)
    {
        rightStickInput = context.ReadValue<Vector2>();

        //selectedPuppy = FindClosestPuppy();

        if (selectedPuppy != null)
        {
            InuMovement inuMovement = selectedPuppy.GetComponent<InuMovement>(); //子犬のスクリプトを取得
            if(inuMovement != null)
            {
                if(rightStickInput.magnitude > stickThreshold)
                {
                    prepareAttackTime = Time.time; //現在の時間の記録を更新する
                    //inuMovement.SetAttackStartedBool(true);
                    inuMovement.SetPuppyState(InuMovement.PuppyState.AttackPreparing); //子犬の状態をAttackPreparingに設定
                    //Debug.Log("preparing attack");


                }
            }
            
        }
    }


    //素早く右スティック入力キャンセルで子犬のLaunchを行う
    private void AttackCanceled(InputAction.CallbackContext context) 
    {
        if (selectedPuppy != null)
        {
            InuMovement inuMovement = selectedPuppy.GetComponent<InuMovement>(); //子犬のスクリプトを取得
            if (inuMovement != null)
            {
                if (inuMovement.GetPuppyState() == InuMovement.PuppyState.AttackPreparing)　//直近までAttackPreparing状態だった（スティックが中心に戻った）
                {
                    if(Time.time - prepareAttackTime > prepareAttackTimeThreshold) // スティックがゆっくり戻った
                    {
                        Debug.Log("Launch Canceled"); //犬の飛び出しは中止する
                        //inuMovement.SetAttackStartedBool(false);
                        inuMovement.SetPuppyState(InuMovement.PuppyState.Following);//子犬はFolloing状態に戻る
                    }
                    else //スティックが素早く戻った
                    {
                        //Debug.Log("Launch Puppy!"); //犬を飛び出させる
                        inuMovement.SetPuppyState(InuMovement.PuppyState.LaunchStart);//子犬はLaunch状態に入る
                    }
                }
            }
        }
    }


    //アタックに使う関数。最も近くにいる子犬を探す。
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
            //子犬がFollowing状態の場合のみ近くにいるか調べる。
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



    //他のスクリプトから右スティックの入力にアクセスするための関数
    public Vector2 GetRightStickInput()
    {
        return rightStickInput;
    }

    //カメラの向きに対して右スティックの入力と逆方向のベクトルを返す。
    //子犬のスクリプトInuMovementから参照する用の関数。
    public Vector3 GetPuppyMoveDirection()
    {
        //Debug.Log("rightStick: " + rightStickInput);
        Vector2 reversedRighStickInput = -rightStickInput;
        float targetAngle = Mathf.Atan2(reversedRighStickInput.x, reversedRighStickInput.y) * Mathf.Rad2Deg + cam.eulerAngles.y;

        //移動方向（ワールド座標で）
        Vector3 puppyMoveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

        return puppyMoveDirection;

    }

}
