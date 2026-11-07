using UnityEngine;
using UnityEngine.UI;





public class EnemyIndicator : MonoBehaviour
{
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private GameObject indicatorContainer;

    [SerializeField] private Camera targetCamera;


    [Header("Utility")]
    private Vector2 screenCenter;
    private GameObject indicator;
    private RectTransform indicatorRectTransform;


    [Header("Settings")]
    private float angleToTheTopRightCorner;


    private void Awake()
    {
        angleToTheTopRightCorner = Mathf.Atan2(Screen.height, Screen.width) * Mathf.Rad2Deg;

        screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        if (screenCenter == Vector2.zero) Debug.LogWarning("ScreenCenter is Vector2.zero");
    }


    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        indicator = Instantiate(indicatorPrefab, indicatorContainer.transform);
        indicator.GetComponentInChildren<Image>().color = GameplayManager.Instance.GetUnitMaterial(GetComponentInParent<HordeFormation>()).color;
        indicatorRectTransform = indicator.GetComponent<RectTransform>();
        indicator.SetActive(true);

        //Debug.Log(this.gameObject.name + " " + targetCamera.WorldToScreenPoint(this.transform.position));
    }



    private void Update()
    {
        ShowIndicator();
    }




    private float indicatorOffsetWhileFacingBorder = 20f;
    private float indicatorOffsetWithSideBorder = 60f;
    private float indicatorOffsetInCorner = 100f;
    private float cornerAngleOffset = 10f;
    private float rightAngle = 90f;

    private float indicatorDisplayCooldownTime = 0.1f;
    private float indicatorDisplayElapsedTime = 0f;
    private void ShowIndicator()
    {
        indicatorDisplayElapsedTime += Time.deltaTime;
        if (indicatorDisplayElapsedTime < indicatorDisplayCooldownTime) return;
        indicatorDisplayElapsedTime = 0f;

        if (targetCamera == null || indicatorPrefab == null || indicator == null) return;

        //Enemy WorldPosToScreenPos
        Vector3 targetInScreenPos = targetCamera.WorldToScreenPoint(transform.position);

        bool isInScreen = targetInScreenPos.x > 0f && targetInScreenPos.x < Screen.width && targetInScreenPos.y > 0f && targetInScreenPos.y < Screen.height;

        if (isInScreen)
        {
            indicator.SetActive(false);
            return;
        }
        else
        {
            indicator.SetActive(true);

            Vector3 directionInWorldSpace = transform.position - GameplayManager.Instance.playerHorde.transform.position;
            Vector3 directionInCameraSpace = targetCamera.transform.InverseTransformDirection(directionInWorldSpace);

            Vector2 screenDirection = new Vector2(directionInCameraSpace.x, directionInCameraSpace.y);
            Vector2 normalVector = screenDirection.normalized;

            float angle = Mathf.Atan2(normalVector.y, normalVector.x) * Mathf.Rad2Deg;
            float angle360 = (angle < 0f) ? angle + 360f : angle;


            Vector3 cappedArrowPosition = targetInScreenPos;

            //Up
            if (angle360 >= angleToTheTopRightCorner + cornerAngleOffset && angle360 < angleToTheTopRightCorner + rightAngle - cornerAngleOffset)
            {
                cappedArrowPosition.y = Screen.height - indicatorOffsetWhileFacingBorder;
                indicator.transform.up = Vector3.up;

                if (cappedArrowPosition.x < 0f) cappedArrowPosition.x = indicatorOffsetWithSideBorder;
                else if (cappedArrowPosition.x > Screen.width) cappedArrowPosition.x = Screen.width - indicatorOffsetWithSideBorder;
            }
            //Top-Right Corner
            else if (angle360 < angleToTheTopRightCorner + cornerAngleOffset && angle360 > angleToTheTopRightCorner - cornerAngleOffset)
            {
                cappedArrowPosition = new Vector2(Screen.width - indicatorOffsetInCorner, Screen.height - indicatorOffsetInCorner);
                indicator.transform.up = new Vector2(1f, 1f).normalized;
            }

            //left
            if (angle360 >= angleToTheTopRightCorner + rightAngle + cornerAngleOffset && angle360 < angleToTheTopRightCorner + (2f * rightAngle) - cornerAngleOffset)
            {
                cappedArrowPosition.x = 0f + indicatorOffsetWhileFacingBorder;
                indicator.transform.up = Vector3.left;

                if (cappedArrowPosition.y < 0f) cappedArrowPosition.y = 0f + indicatorOffsetWithSideBorder;
                else if (cappedArrowPosition.y > Screen.height) cappedArrowPosition.y = Screen.height - indicatorOffsetWithSideBorder;
            }
            //Top-Left Corner
            else if (angle360 > angleToTheTopRightCorner + rightAngle - cornerAngleOffset && angle360 < angleToTheTopRightCorner + rightAngle + cornerAngleOffset)
            {
                cappedArrowPosition = new Vector2(0f + indicatorOffsetInCorner, Screen.height - indicatorOffsetInCorner); 
                indicator.transform.up = new Vector2(-1f, 1f).normalized;
            }

            //Down
            if (angle360 >= angleToTheTopRightCorner + (2f * rightAngle) + cornerAngleOffset && angle360 < angleToTheTopRightCorner + (3f * rightAngle) - cornerAngleOffset)
            {
                cappedArrowPosition.y = 0f + indicatorOffsetWhileFacingBorder;
                indicator.transform.up = Vector3.down;

                if (cappedArrowPosition.x < 0f) cappedArrowPosition.x = 0f + indicatorOffsetWithSideBorder;
                else if (cappedArrowPosition.x > Screen.width) cappedArrowPosition.x = Screen.width - indicatorOffsetWithSideBorder;
            }
            //Bottom-Left Corner
            else if (angle360 > angleToTheTopRightCorner + (2f * rightAngle) - cornerAngleOffset && angle360 < angleToTheTopRightCorner + (2f * rightAngle) + cornerAngleOffset)
            {
                cappedArrowPosition = new Vector3(0f + indicatorOffsetInCorner, 0f + indicatorOffsetInCorner);
                indicator.transform.up = new Vector2(-1f, -1f).normalized;
            }

            //Right
            if (angle360 >= angleToTheTopRightCorner + (3f * rightAngle) + cornerAngleOffset || angle360 < angleToTheTopRightCorner - cornerAngleOffset)
            {
                cappedArrowPosition.x = Screen.width - indicatorOffsetWhileFacingBorder;
                indicator.transform.up = Vector3.right;

                if (cappedArrowPosition.y < 0f) cappedArrowPosition.y = 0f + indicatorOffsetWithSideBorder;
                else if (cappedArrowPosition.y > Screen.height) cappedArrowPosition.y = Screen.height - indicatorOffsetWithSideBorder;
            }
            //Bottom-Right Corner
            else if (angle360 < angleToTheTopRightCorner + (3f * rightAngle) + cornerAngleOffset && angle360 > angleToTheTopRightCorner + (3f * rightAngle) - cornerAngleOffset)
            {
                cappedArrowPosition = new Vector3(Screen.width + indicatorOffsetInCorner, 0f + indicatorOffsetInCorner);
                indicator.transform.up = new Vector2(1f, -1f).normalized;
            }

            indicatorRectTransform.position = cappedArrowPosition;
        }
    }



    /*private void ShowIndicator()
    {
        if (targetCamera == null || indicatorPrefab == null || indicator == null) return;

        //Enemy WorldPosToScreenPos
        Vector3 targetInScreenPos = targetCamera.WorldToScreenPoint(transform.position);

        bool isInScreen = targetInScreenPos.x > 0f && targetInScreenPos.x < Screen.width && targetInScreenPos.y > 0f && targetInScreenPos.y < Screen.height;

        if (isInScreen)
        {
            indicator.SetActive(false);
            return;
        }
        else
        {
            indicator.SetActive(true);

            Vector3 directionInWorldSpace = transform.position - GameplayManager.Instance.playerHorde.transform.position;
            Vector3 directionInCameraSpace = targetCamera.transform.InverseTransformDirection(directionInWorldSpace);

            Vector2 screenDirection = new Vector2(directionInCameraSpace.x, directionInCameraSpace.y);
            Vector2 normalVector = screenDirection.normalized;

            float angle = Mathf.Atan2(normalVector.y, normalVector.x) * Mathf.Rad2Deg;
            Debug.Log(gameObject.name + " " + angle);
            Vector3 cappedArrowPosition = targetInScreenPos;


            if (angle >= angleToTheTopRightCorner + 10f && angle < 90f - 10f + angleToTheTopRightCorner)
            {
                cappedArrowPosition.y = Screen.height - indicatorOffsetWhileFacingBorder;
                indicator.transform.up = Vector3.up;
                if (cappedArrowPosition.x < 0f) cappedArrowPosition.x = indicatorOffsetWithSideBorder;
                else if (cappedArrowPosition.x >= Screen.width) cappedArrowPosition.x = Screen.width - indicatorOffsetWithSideBorder;
            }
            else if (angle >= angleToTheTopRightCorner - 10f && (angle < angleToTheTopRightCorner + 10f || angle < angleToTheTopRightCorner + 360f + 10f)) //Top-Right
            {
                indicator.transform.up = new Vector2(1f, 1f).normalized;
                cappedArrowPosition = new Vector2(Screen.width - indicatorOffsetInCorner, Screen.height - indicatorOffsetInCorner);
            }


            if ((angle >= angleToTheTopRightCorner + 90f + 10f && angle <= 180f) || (angle < -angleToTheTopRightCorner - 90f - 10f) && (angle >= -180f))
            {
                cappedArrowPosition.x = indicatorOffsetWhileFacingBorder;
                indicator.transform.up = Vector3.left;
                if (cappedArrowPosition.y < 0f) cappedArrowPosition.y = indicatorOffsetWithSideBorder;
                else if (cappedArrowPosition.y >= Screen.height) cappedArrowPosition.y = Screen.height - indicatorOffsetWithSideBorder;
            }
            else if (angle >= angleToTheTopRightCorner + 90f - 10f && angle < angleToTheTopRightCorner + 90f + 10f)
            {
                indicator.transform.up = new Vector2(-1f, 1f).normalized;
                cappedArrowPosition = new Vector3(0f + indicatorOffsetInCorner, Screen.height - indicatorOffsetInCorner);
            }

            if (angle <= -angleToTheTopRightCorner - 10f && angle >= -angleToTheTopRightCorner - 90f + 10f)
            {
                cappedArrowPosition.y = indicatorOffsetWhileFacingBorder;
                indicator.transform.up = Vector3.down;
                if (cappedArrowPosition.x < 0f) cappedArrowPosition.x = indicatorOffsetWithSideBorder;
                else if (cappedArrowPosition.x >= Screen.width) cappedArrowPosition.x = Screen.width - indicatorOffsetWithSideBorder;
            }
            else if (angle < -angleToTheTopRightCorner + 10f && angle > -angleToTheTopRightCorner - 10f)
            {
                indicator.transform.up = new Vector2(-1f, -1f).normalized;
                cappedArrowPosition = new Vector3(0f + indicatorOffsetInCorner, 0f + indicatorOffsetInCorner);
            }


            if ((angle >= (-angleToTheTopRightCorner + 10f) && (angle < 0f)) || (angle < (angleToTheTopRightCorner - 10f) && (angle >= 0f)))
            {
                cappedArrowPosition.x = Screen.width - indicatorOffsetWhileFacingBorder;
                indicator.transform.up = Vector3.right;
                if (cappedArrowPosition.y < 0f) cappedArrowPosition.y = indicatorOffsetWithSideBorder;
                else if (cappedArrowPosition.y >= Screen.height) cappedArrowPosition.y = Screen.height - indicatorOffsetWithSideBorder;
            }
            else if (angle >= -angleToTheTopRightCorner - 10f && angle < - angleToTheTopRightCorner + 10f)
            {
                indicator.transform.up = new Vector2(1f, -1f).normalized;
                cappedArrowPosition = new Vector3(Screen.width - indicatorOffsetInCorner, 0f + indicatorOffsetInCorner);
            }

            indicatorRectTransform.position = cappedArrowPosition;
        }
    }*/
}
