#region usings
using System;
using System.Collections;
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
	[PluginInfo(Name = "LifeValue", Category = "Value", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class ValueLifeValueNode : IPluginEvaluate
	{

		private float defaultSpeed;
		private ISpread<double> life;
		private ISpread<double> speed;
		private ArrayList pos;
		
		ValueLifeValueNode(){
			defaultSpeed = 0.001f;
			pos = new ArrayList();
		}
		
		#region fields & pins
		[Input("Bang" , DefaultValue = 0)]
		public ISpread<Boolean> Bang;

		[Input("Reset" , DefaultValue = 0)]
		public ISpread<Boolean> Reset;
		
		[Input("Input", DefaultValue = 1.0)]
		public ISpread<double> FInput;

		[Input("SlowSpeed", DefaultValue = 0.0005)]
		public ISpread<float> SlowSpeed;
		
		[Input("Speed", DefaultValue = 1.0)]
		public ISpread<float> Speed;

		[Input("SlowRangeFrom", DefaultValue = 0.0)]
		public ISpread<double> From;

		[Input("SlowRangeTo", DefaultValue = 0.0)]
		public ISpread<double> To;

		
		[Output("Output")]
		public ISpread<float> FOutput;

		//[Output("Count")]
		//public int Count;
		
		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			//FOutput.SliceCount = SpreadMax;
			FOutput.SliceCount = pos.Count;
			
			ArrayList newList = new ArrayList();
			
			if(Reset[0]){
				resetValue();
			}			
			
			if(Bang[0]){
				addValue();
			}
			
			/*
			for (int i = 0; i < SpreadMax; i++)
				FOutput[i] = FInput[i] * 2;
			*/
			
			for(int i = 0;i < pos.Count; i++){
				float x = (float)pos[i];
				
				if(x > From[0] && x < To[0]){
					//slow speed range
					x = x + SlowSpeed[0];
				}else{
					x = x + Speed[0];
				}
				
				if(x < 1.0){
					pos[i] = x;
					newList.Add(x);
				}else{
					FLogger.Log(LogType.Debug, "del:" + i + " x:" + x);
				}
				//FOutput[i] = (float)pos[i];
			}
			//FOutput = (ISpread<float>)newList.Clone();
			FOutput.SliceCount = newList.Count;	
			pos = new ArrayList();
			for(int i = 0;i < newList.Count; i++){
				FOutput[i] = (float)newList[i];
				pos.Add(newList[i]);
			}				
		}
		
		/**
		*
		*/
		private void addValue(){
			FLogger.Log(LogType.Debug, "addValue");
			
			pos.Add(0.0f);
			FLogger.Log(LogType.Debug, "pos.Count:" + pos.Count);
			/*
			for(int i = 0;i < pos.Count; i++){
				FLogger.Log(LogType.Debug, "pos:" + pos[i]);
			}
			*/

		}
		
		/**
		*
		*/
		private void resetValue(){
			FLogger.Log(LogType.Debug, "resetValue");
			pos = new ArrayList();
			
		}		
		
	}
}
