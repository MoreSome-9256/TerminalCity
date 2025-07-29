using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowMouse : MonoBehaviour
{
	public float xSpeed = 2.0f; // 控制相机在X轴上的移动速度
	public float ySpeed = 2.0f; // 控制相机在Y轴上的移动速度

	public float xMinLimit = -10.0f; // X轴上的最小限制
	public float xMaxLimit = 10.0f;  // X轴上的最大限制
	public float yMinLimit = -5.0f; // Y轴上的最小限制
	public float yMaxLimit = 5.0f;  // Y轴上的最大限制

	private Vector3 cameraPosition; // 存储相机的当前位置

	// Start is called before the first frame update
	void Start()
	{
		cameraPosition = transform.position; // 初始化相机位置
	}

	// Update is called once per frame
	void Update()
	{
		// 根据鼠标移动来计算相机的新位置
		float mouseX = Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
		float mouseY = Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;

		// 更新相机的X和Y位置，但限制在指定的范围内
		cameraPosition.x = Mathf.Clamp(cameraPosition.x + mouseX, xMinLimit, xMaxLimit);
		cameraPosition.y = Mathf.Clamp(cameraPosition.y + mouseY, yMinLimit, yMaxLimit);

		// 设置相机的新位置
		transform.position = cameraPosition;
	}
}