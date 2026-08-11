
#define FMOD_LOGGING

using FMOD.Studio;

namespace panpan
{
    public static class AudioSystem
    {
        #region Private-Variables
        static FMOD.Studio.System fmodStudioSystem;
        static FMOD.System fmodSystem;
        #endregion

        #region Public-Methods

        public static void Init()
        {
            InitFMOD();
        }

        public static void Update()
        {
            fmodStudioSystem.update();
        }

        public static void Play(FMOD.GUID ev)
        {
            fmodStudioSystem.getEventByID(ev, out EventDescription evDesc);
            evDesc.createInstance(out EventInstance evInst);
            evInst.start();
            evInst.release();
        }
        #endregion

        #region Private-Methods
        private static void InitFMOD()
        {
            FMOD.Studio.System.create(out fmodStudioSystem);
            fmodStudioSystem.getCoreSystem(out fmodSystem);
            fmodSystem.setDSPBufferSize(256,4);
            fmodStudioSystem.initialize(128, FMOD.Studio.INITFLAGS.NORMAL, FMOD.INITFLAGS.NORMAL, (IntPtr)0);
            
            FMOD.RESULT res = fmodStudioSystem.loadBankFile("/home/casey/dev/cardavan_panpan/fmod/cardavan/Build/Desktop/Master.bank", FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out Bank bank);
            if(res != FMOD.RESULT.OK)
            {
                Log.Error($"Error loading bank file: {res}", "FMOD");
            }
            else
            {
                Log.Info("Bank Loaded", "FMOD");
            }
            res = fmodStudioSystem.loadBankFile("/home/casey/dev/cardavan_panpan/fmod/cardavan/Build/Desktop/Master.strings.bank", FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out Bank strings);
            if(res != FMOD.RESULT.OK)
            {
                Log.Error($"Error loading bank strings file: {res}", "FMOD");
            }
            else
            {
                Log.Info("Bank strings loaded", "FMOD");
            }
        }
        #endregion
    }
}