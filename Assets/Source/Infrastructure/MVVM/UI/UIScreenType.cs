using System;

namespace Source.Infrastructure.MVVM.UI
{
    [Serializable]
    public enum UIScreenType
    {
        None = 0,
        MainMenu = 1,
        CreateSessionPopUp = 2,
    }

    public static class UIScreenTypeMapper
    {
        public static string GetAddress(this UIScreenType screenType)
        {
            return screenType switch
            {
                UIScreenType.None => string.Empty,
                UIScreenType.MainMenu => ScreenAddressConstants.MainMenuScreen,
                UIScreenType.CreateSessionPopUp => ScreenAddressConstants.CreateSessionPopUpScreen,
                _ => string.Empty,
            };
        }
    }
}