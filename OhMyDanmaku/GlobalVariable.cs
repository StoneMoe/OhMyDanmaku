namespace OhMyDanmaku
{
    /// <summary>
    /// User Settings Storage
    /// </summary>
    class GlobalVariable
    {
        //Render - User Settings
        public static double _user_render_width;
        public static double _user_render_height;

        //Danmaku - User Settings
        public static int _user_danmaku_Duration;
        public static int _user_danmaku_FontSize;
        public static bool _user_danmaku_EnableShadow;
        public static bool _user_danmaku_EnableOutline;

        public static byte _user_danmaku_colorR;
        public static byte _user_danmaku_colorG;
        public static byte _user_danmaku_colorB;

        //Communication - User Settings
        public static int _user_com_port;
        public static bool _user_audit_enabled;
    }
}
