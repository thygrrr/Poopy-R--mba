using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footprint : MonoBehaviour
{
	public int[] data;

	private bool[,] _mask = null;

	public bool[,] mask { get { return buildMask(); } }

	private bool[,] buildMask()
	{
		if (_mask == null)
		{
			int height = data.Length;
			int width = 0;

			foreach (int row in data)
			{
				width = (int)Mathf.Max(width, Mathf.Floor(Mathf.Log10(row)+1));
			}

			_mask = new bool[width, height];
			for (int y = height - 1; y >= 0; y--)
			{
				int bits = data[y];
				for (int x = width - 1; x >= 0; x--)
				{
					_mask[x, y] = bits % 10 > 0;
					bits /= 10;
				}
			}
		}

		return _mask;
	}
}
