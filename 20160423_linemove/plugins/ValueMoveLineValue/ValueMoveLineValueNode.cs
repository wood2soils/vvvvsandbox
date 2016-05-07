#region usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "MoveLineValue", Category = "Value", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class ValueMoveLineValueNode : IPluginEvaluate
	{
		
		bool initFlg;
		List<float> life;
		List<bool> startFlg;
		
		ValueMoveLineValueNode(){
			
			initFlg=true;
			//life = new ArrayList();
			//startFlg = new ArrayList();
		}
		
		#region fields & pins
		[Input("Bang" , DefaultBoolean = false)]
		public ISpread<Boolean> Bang;

		[Input("Reset" , DefaultBoolean = false)]
		public ISpread<Boolean> Reset;
		
		
		[Input("MaxLength", DefaultValue = 1.0)]
		public ISpread<double> MaxLength;

		[Input("MaxLife", DefaultValue = 1.0)]
		public ISpread<double> MaxLife;

		[Input("Range", DefaultValue = 1.0)]
		public ISpread<double> Range;
		
		
		[Input("StartPos")]
		public ISpread<Vector3D> StartPos;
		
		[Input("Dir")]
		public ISpread<Vector3D> Dir;

		[Output("P1")]
		public ISpread<Vector3D> P1;

		[Output("P2")]
		public ISpread<Vector3D> P2;
		
		
		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			P1.SliceCount = SpreadMax;
			
			int n = P1.SliceCount;
			if(startFlg == null || n != startFlg.Count){
				
				FLogger.Log(LogType.Debug, "init");				
				startFlg = new List<bool>();
				life = new List<float>();
				for(int i=0;i<n;i++){
					startFlg.Add(false);
					life.Add(0);
				}
			}
			if(Reset[0]){
				initFlg = true;
				for(int i=0;i<n;i++){
					life[i] = 0.0f;
					startFlg[i] = true;
					FLogger.Log(LogType.Debug, "add:" + i);				

				}
			}
			if(Bang[0]){
				//add point
				startFlg.Add(false);
				life.Add(0);				
			}

			for (int i = 0; i < SpreadMax; i++){

				if(startFlg[i]){
					life[i] = (float)life[i]+0.01f;
					if(life[i] > MaxLife[i]){
						P1[i] = P1[i] + Dir[i];
						if((P1[i] - P2[i]).Length < 0.001){
							
							startFlg[i] = false;
						}
						continue;
					}
					if(initFlg){
						P1[i] = StartPos[i];
						P2[i] = StartPos[i];
						life.Add(0.0f);
						initFlg = false;
					}else{
						P2[i] = P2[i] + Dir[i];
					}
					//
					double distance = (P1[i] - P2[i]).Length;					
					if(distance > MaxLength[i]){
						P1[i] = P1[i] + Dir[i];
					}
					
				}
			}

			//FLogger.Log(LogType.Debug, "hi tty!");
		}
		
		
	}
}
