﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour {
	// Start is called before the first frame update
	const float t = 0.1f;
	private float dt;
	private Color originalColor;
	private Color currentColor;
	private Color minimumColor;
	private Vector3 originalSize;
	private Renderer rend;
	GameObject sun;
	void Start() {
		dt = t * Time.deltaTime;
		rend = GetComponent<Renderer>();
		originalColor = rend.material.color;
		minimumColor = originalColor / 5;
		originalSize = transform.localScale;
		currentColor = originalColor;
		transform.localScale = transform.localScale / 2;
		sun = GameObject.Find("Sun");
		if (sun == null) {
			Debug.LogAssertion("Sun is null in clouds.");
		}
	}

	// Update is called once per frame
	void Update() {

		float offset = 0.013f;	// movement offset
		Vector3 minimumSize = originalSize / 4;
		float sunPosition = sun.transform.position.y;

		// if it is daytime
		if (sunPosition > 200) {
			// increase cloud sizes
			transform.localScale += new Vector3(dt * 0.1f, dt * 0.1f, dt * 0.1f);

			// move could in random x direction with 85% in positive x
			int sign = -1;
			if (Random.Range(0, 100) < 85) {
				sign = 1;
			}
			transform.Translate(offset * sign, 0, 0);

		// if it is nighttime
		} else {
			// Decrese cloud sizes up to a minimum
			transform.localScale -= new Vector3(dt * 0.1f, dt * 0.1f, dt * 0.1f);
			if (transform.localScale.x < minimumSize.x || transform.localScale.y < minimumSize.y || transform.localScale.z < minimumSize.z) {
				transform.localScale = minimumSize;
			}
			// move clouds in a random x direction
			int sign = 1;
			if (Random.Range(0, 100) < 85) {
				sign = -1;
			}
			transform.Translate(offset * sign, 0, 0);
		}

		// Change color
		// start changing color when the sun sets
		float sunOffsetColor = sunPosition - 200;
		float changeColorOffset = 0.0000001f;
		currentColor.r += changeColorOffset * sunOffsetColor;
		currentColor.g += changeColorOffset * sunOffsetColor;
		currentColor.b += changeColorOffset * sunOffsetColor;

		// Do not go brighter than original
		if (currentColor.r > originalColor.r) {
			currentColor = originalColor;
		} else if (currentColor.r < minimumColor.r) {
			currentColor = minimumColor;
		}
		rend.material.color = currentColor;

	}
}
