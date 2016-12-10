using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(PicaVoxel.Volume))]
public class PatternGenerator : MonoBehaviour
{
	[Range(0.0f, 1.0f)]
	public float noise = 0.0f;

	public Color[] colors;
}
