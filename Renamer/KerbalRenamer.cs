/*
Copyright (c) 2014~2016, Justin Bengtson
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

  Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

  Redistributions in binary form must reproduce the above copyright notice, this
  list of conditions and the following disclaimer in the documentation and/or
  other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System.Collections.Generic;
using System.Collections;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;

namespace Renamer {
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class KerbalRenamer : MonoBehaviour {
		public static KerbalRenamer rInstance = null;
		public string cultureDescriptor = "Culture";
		internal Culture[] cultures = {};

        public List<string> originalNames = new List<string> {
                "Jebediah Kerman",
                "Bill Kerman",
                "Bob Kerman",
                "Valentina Kerman"
            };

        public static KerbalRenamer Instance {
	        get {
				if(rInstance == null) {
					rInstance = (new GameObject("RenamerContainer")).AddComponent<KerbalRenamer>();
				}
	            return rInstance;
	        }
	    }
       
        public void Awake()
        {
            DontDestroyOnLoad(this);

            ConfigNode data = null;
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("KERBALRENAMER"))
            {
                data = node;
            }
            if (data == null)
            {
                Debug.Log("KerbalRenamer: No config file found, thanks for playing.");
                return;
            }

            List<Culture> ctemp = new List<Culture>();            
            if (data.HasValue("cultureDescriptor"))
            {
                cultureDescriptor = data.GetValue("cultureDescriptor");
            }
            ConfigNode[] cultureclub = data.GetNodes("Culture");
            for (int i = 0; i < cultureclub.Length; i++)
            {
                Culture c = new Culture(cultureclub[i]);
                ctemp.Add(c);
            }
            cultures = ctemp.ToArray();

            GameEvents.onKerbalAdded.Add(new EventData<ProtoCrewMember>.OnEvent(OnKerbalAdded));
            GameEvents.onGameStateCreated.Add(new EventData<Game>.OnEvent(OnGameCreated));
        }

        void OnGameCreated(Game game)
        {

        }

        public void OnKerbalAdded(ProtoCrewMember kerbal)
		{
			if (RenamerCustomParams.PreserveOriginal4Enabled) {
				if (originalNames.Contains(kerbal.name)) {
					return;
				}
			}
            else // see if any of the originals are still around
            {
                RerollOriginals();
            }

            Randomizer.RerollKerbal(ref kerbal, cultures);
		}
        

        private void RerollOriginals()
        {
            foreach (var originalKerbalName in originalNames)
            {
                if (HighLogic.CurrentGame.CrewRoster[originalKerbalName] != null)
                {
                    var origKerbal = HighLogic.CurrentGame.CrewRoster[originalKerbalName];                    
                    Randomizer.RerollKerbal(ref origKerbal, cultures);                    
                }
            }
        }        
	}

}