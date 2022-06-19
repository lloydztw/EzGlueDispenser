using JetEazy.BasicSpace;



namespace Eazy_Project_III.ProcessSpace
{
    /// <summary>
    /// 蜂鳴器 <br/>
    /// @LETIAN: 20220619 (Refactor)
    /// </summary>
    public class BuzzerProcess : BaseProcess
    {
        #region PRIVATE_DATA
        /// <summary>
        /// 叫的第几次
        /// </summary>
        int m_BuzzerIndex = 0;
        /// <summary>
        /// 叫几声
        /// </summary>
        int m_BuzzerCount = 3;
        #endregion

        #region SINGLETON
        static BuzzerProcess _singleton = null;
        private BuzzerProcess()
        {
        }
        #endregion

        public static BuzzerProcess Instance
        {
            get
            {
                if (_singleton == null)
                    _singleton = new BuzzerProcess();
                return _singleton;
            }
        }

        /// <summary>
        /// 第一個參數可以指定 m_BuzzerCount
        /// </summary>
        /// <param name="args">args[0] 可以指定 m_BuzzerCount</param>
        public override void Start(params object[] args)
        {
            m_BuzzerIndex = 0;
            m_BuzzerCount = 3;
            try
            {
                if (args.Length > 0)
                    m_BuzzerCount = (int)args[0];
            }
            catch
            {

            }
            ((ProcessClass)this).Start();
        }

        public override void Tick()
        {
            var Process = this;

            //iNextDurtime[3] = 1000;

            if (Process.IsOn)
            {
                switch (Process.ID)
                {
                    case 5:

                        Process.NextDuriation = 100;
                        Process.ID = 10;

                        switch (Process.RelateString)
                        {
                            default:
                                m_BuzzerIndex = 0;
                                //m_BuzzerCount = 3;
                                break;
                        }

                        break;
                    case 10:
                        if (Process.IsTimeup)
                        {
                            if (m_BuzzerIndex < m_BuzzerCount)
                            {
                                MACHINE.PLCIO.ADR_BUZZER = true;

                                Process.NextDuriation = 500;
                                Process.ID = 15;

                                m_BuzzerIndex++;
                            }
                            else
                            {
                                MACHINE.PLCIO.ADR_BUZZER = false;

                                Process.Stop();
                            }
                        }
                        break;
                    case 15:
                        if (Process.IsTimeup)
                        {
                            MACHINE.PLCIO.ADR_BUZZER = false;

                            Process.NextDuriation = 500;
                            Process.ID = 10;
                        }
                        break;
                }
            }
        }


    }
}
