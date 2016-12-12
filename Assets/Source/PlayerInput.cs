using Entitas;
using UnityEngine;

public class PlayerInput : IExecuteSystem, ISetPool
{
	private Group _receivers;

	public void SetPool(Pool pool)
	{
		_receivers = pool.GetGroup(Matcher.AllOf(Matcher.InputReceiver).NoneOf(Matcher.Move));
	}


	public void Execute()
	{
		AudioSource audio = Camera.main.GetComponent<AudioSource>();
		Sounds sounds = Camera.main.GetComponent<Sounds>();

		foreach (var receiver in _receivers.GetEntities())
		{
			if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
			{
				receiver.AddMove(Move.Direction.Up);
				audio.clip = receiver.isDirty ? sounds.dirty : sounds.clean;
				audio.Play();
			}
			else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
			{
				receiver.AddMove(Move.Direction.Left);
				audio.clip = receiver.isDirty ? sounds.dirty : sounds.clean;
				audio.Play();
			}
			else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
			{
				receiver.AddMove(Move.Direction.Down);
				audio.clip = receiver.isDirty ? sounds.dirty : sounds.clean;
				audio.Play();
			}
			else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
			{
				receiver.AddMove(Move.Direction.Right);
				audio.clip = receiver.isDirty ? sounds.dirty : sounds.clean;
				audio.Play();
			}
		}
	}
}