namespace MarketingBox.Bridge.Capartners.Service.Services.Integrations.Contracts.Enums
{
    public enum SimpleTradingResultCode
    {
        /// <summary>
        /// PersonalData Not Valid
        /// </summary>
        SystemError = -8,

        /// <summary>
        /// PersonalData Not Valid
        /// </summary>
        PersonalDataNotValid = -7,

        /// <summary>
        /// File Not Found
        /// </summary>
        FileNotFound = -6,

        /// <summary>
        /// File Wrong Extension
        /// </summary>
        FileWrongExtension = -5,

        /// <summary>
        /// Old password Not Match
        /// </summary>
        OldPasswordNotMatch = -4,

        /// <summary>
        /// User not exist
        /// </summary>
        UserNotExist = -3,

        /// <summary>
        /// User already exists
        /// </summary>
        UserExists = -2,
        /// <summary>
        /// Invalid username or password
        /// </summary>
        InvalidUserNameOrPassword = -1,

        /// <summary>
        /// Ok
        /// </summary>
        Ok
    }

}
