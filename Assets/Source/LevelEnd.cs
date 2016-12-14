using System.Collections.Generic;
using Entitas;
using UnityEngine;

public class LevelEnd : IExecuteSystem, ISetPool
{
	private Pool _pool;
	private float duration = 0;

	public void SetPool(Pool pool)
	{
		_pool = pool;
	}
	

	public void Execute()
	{
		if (duration > 0)
		{
			duration -= Time.deltaTime;

			if (duration <= 0)
			{
				_pool.isNextLevel = true;
				GameObject.FindGameObjectWithTag("Missed").GetComponent<Animator>().SetBool("Shown", false);
				GameObject.FindGameObjectWithTag("Full").GetComponent<Animator>().SetBool("Shown", false);
			}
		}
		else
		{
			AudioSource audio = Camera.main.GetComponent<AudioSource>();
			Sounds sounds = Camera.main.GetComponent<Sounds>();

			if (_pool.isSuccess)
			{
				//Play sound.
				audio.clip = sounds.success;
				audio.Play();

				GameObject.FindGameObjectWithTag("Full").GetComponent<Animator>().SetBool("Shown", true);
				duration = 2.0f;
				_pool.isSuccess = false;
			}

			if (_pool.isFailure)
			{
				//Play sound.
				audio.clip = sounds.failure;
				audio.Play();

				GameObject.FindGameObjectWithTag("Missed").GetComponent<Animator>().SetBool("Shown", true);
				duration = 2.0f;
				_pool.isFailure = false;
			}
		}
	}
}