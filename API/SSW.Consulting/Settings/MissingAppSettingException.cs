﻿using System;

namespace SSW.Consulting.WebAPI.Settings
{
	public class MissingAppSettingException : Exception
	{
		public string AppSettingName { get; }

		public MissingAppSettingException(string appSettingName) : base($"The required AppSetting {appSettingName} is missing from the app configuration!")
		{
			AppSettingName = appSettingName;
		}
	}
}