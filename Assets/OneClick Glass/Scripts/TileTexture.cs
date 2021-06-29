using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public class TileTexture : MonoBehaviour {
	public Material _mat;
	public Texture[] _texture;
	public string textureName;
	[Range(0, 50)]
	public int _speed;

	[SerializeField]private float _currentTex;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (_currentTex < _texture.Length) {
			_currentTex = Mathf.MoveTowards(_currentTex, _texture.Length, _speed*Time.unscaledDeltaTime);
		} else
			_currentTex = 1;
		if (textureName == null) {
			_mat.mainTexture = _texture [(int)_currentTex - 1];
		}else
			_mat.SetTexture(textureName, _texture[(int)_currentTex -1]);

	}
}
