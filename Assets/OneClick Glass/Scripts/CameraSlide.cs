using UnityEngine;
using System.Collections;

public class CameraSlide : MonoBehaviour {
	private Vector3 _position;
	public int _segmentCount;
	public int _segment;
	public float _speed;
	private bool _start;
	private bool _mirrors;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = _position;

		if (_segment != -1) {			
			_position = new Vector3 (Mathf.MoveTowards (_position.x, _segment * 10, _speed * 10 * Time.unscaledDeltaTime), 1, -10);
		
		}	

	


		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			if (_segment  <=_segmentCount -1) {
				_segment += 1;
				_start = false;
			} else
				_segment = 0;
			_start = true;
		}

		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
		
			if (_segment <= _segmentCount && _segment >= 1) {
				_segment -= 1;
				_start = false;
			}

		}



	}
}
