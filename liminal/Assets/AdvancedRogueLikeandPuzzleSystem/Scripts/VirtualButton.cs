﻿using UnityEngine;

namespace AdvancedRogueLikeandPuzzleSystem
{
	public class VirtualButton
	{
		public string Name { get; set; }

		public bool IsPressed { get; private set; }

		private int _lastPressedFrame = -1;

		private int _lastReleasedFrame = -1;

		public VirtualButton(string name)
		{
			Name = name;
		}

		public void Press()
		{
			if (IsPressed)
			{
				return;
			}
			IsPressed = true;
			_lastPressedFrame = Time.frameCount;
		}

		public void Release()
		{
			IsPressed = false;
			_lastReleasedFrame = Time.frameCount;
		}

		public bool GetButton
		{
			get { return IsPressed; }
		}

		public bool GetButtonDown
		{
			get
			{
				return _lastPressedFrame != -1 && _lastPressedFrame - Time.frameCount == -1;
			}
		}

		public bool GetButtonUp
		{
			get
			{
				return _lastReleasedFrame != -1 && _lastReleasedFrame == Time.frameCount - 1;
			}
		}
	}
}