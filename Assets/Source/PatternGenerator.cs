using System.Collections;
using System.Collections.Generic;
using PicaVoxel;
using UnityEngine;


[RequireComponent(typeof(PicaVoxel.Volume))]
public class PatternGenerator : MonoBehaviour
{
	[Range(0.0f, 1.0f)]
	public float noise = 0.0f;

	public Color[] colors;

	public VoxelState state = VoxelState.Active;

	private Color Fuzz(Color c)
	{
		var l = Random.Range(-noise, noise);
		return new Color(c.r + l, c.g + l, c.b + l);
	}

	void Awake()
	{
		Volume volume = transform.GetComponent<Volume>();

		for (int x = 0; x < volume.XSize; x++)
		{
			for (int z = 0; z < volume.ZSize; z++)
			{
				volume.SetVoxelAtArrayPosition(new PicaVoxelPoint(x, 0, z),
					new Voxel()
					{
						State = state,
						Color = Fuzz(colors[Random.Range(0, colors.Length)])
					}
				);
			}
		}

		volume.Frames[0].UpdateAllChunks();

	}
}
