
#define FMOD_LOGGING

using System.Reflection;
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

        public static bool IsPlaying(FMOD.GUID ev)
        {
            fmodStudioSystem.getEventByID(ev, out EventDescription evDesc);
            evDesc.getInstanceCount(out int count);
            return count > 0;
        }
        public static bool IsPlaying(EventInstance evInst)
        {
            evInst.getPlaybackState(out PLAYBACK_STATE state);
            return (state & PLAYBACK_STATE.PLAYING | PLAYBACK_STATE.STARTING) != 0;
        }

        public static EventInstance Play(FMOD.GUID ev)
        {
            fmodStudioSystem.getEventByID(ev, out EventDescription evDesc);
            evDesc.createInstance(out EventInstance evInst);
            evInst.start();
            evInst.release();
            return evInst;
        }
        #endregion

        #region Private-Methods
        private static void InitFMOD()
        {
            FMOD.Studio.System.create(out fmodStudioSystem);
            fmodStudioSystem.getCoreSystem(out fmodSystem);
            fmodSystem.setDSPBufferSize(256,4);
            fmodStudioSystem.initialize(128, FMOD.Studio.INITFLAGS.NORMAL | INITFLAGS.LIVEUPDATE, FMOD.INITFLAGS.NORMAL, (IntPtr)0);

            Assembly asm = Assembly.GetExecutingAssembly();
            string path = System.IO.Path.GetDirectoryName(asm.Location);

            
            FMOD.RESULT res = fmodStudioSystem.loadBankFile(Path.Join(path,"Master.bank"), FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out Bank bank);
            if(res != FMOD.RESULT.OK)
            {
                Log.Error($"Error loading bank file: {res}", "FMOD");
            }
            else
            {
                Log.Info("Bank Loaded", "FMOD");
            }
            res = fmodStudioSystem.loadBankFile(Path.Join(path,"Master.strings.bank"), FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out Bank strings);
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