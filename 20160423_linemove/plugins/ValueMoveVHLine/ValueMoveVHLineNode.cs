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
	[PluginInfo(Name = "MoveVHLine", Category = "Value", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class MoveVHLineNode : IPluginEvaluate
	{

		private float defaultSpeed;
		private ISpread<double> life;
		private ISpread<double> speed;
		private List<List<Vector3D>> posList;
		private List<List<int>> dirList;
		private List<double> unitLengthList;
		private ISpread<ArrayList> initFlgList;
		private Random cRandom;
		private bool isInit;
		
		MoveVHLineNode(){
			//init();
			isInit = true;			
		}
		
		private void init(){
			
			posList = new List<List<Vector3D>>();
			dirList = new List<List<int>>();
			unitLengthList = new List<double>();
			
			//FLogger.Log(LogType.Debug, "LineCount[0]:" + LineCount[0] + " posList.Count:" + posList.Count);
			changeNum();
			
			defaultSpeed = 0.001f;
			cRandom = new System.Random();
			
		}
		
		
		private void changeNum(){
			
			for(int i=0;i<LineCount[0]-posList.Count;i++){
				FLogger.Log(LogType.Debug, "i:" + i);

				List<Vector3D> tmpPos = new List<Vector3D>();
				List<int> tmpDir = new List<int>();

				posList.Add(tmpPos);
				dirList.Add(tmpDir);
				unitLengthList.Add(0.1);
			}
		}
		
		
		#region fields & pins
		[Input("Bang" , DefaultValue = 0)]
		public ISpread<Boolean> Bang;

		[Input("Reset" , DefaultValue = 0)]
		public ISpread<Boolean> Reset;

		[Input("Stop" , DefaultValue = 0)]
		public ISpread<Boolean> Stop;
		
		[Input("StartPos")]
		public ISpread<Vector3D> StartPos;
		
		[Input("MaxPointNum", DefaultValue = 10)]
		public ISpread<int> MaxPointNum;

		[Input("LineCount", DefaultValue = 1)]
		public ISpread<int> LineCount;
		
		
		[Input("Speed", DefaultValue = 0.005)]
		public ISpread<float> Speed;

		[Output("Life")]
		public ISpread<float> Life;

		[Output("Count")]
		public ISpread<int> Count;
		
		[Output("LeadPoint")]
		public ISpread<Vector3D> LeadPoint;
		
		[Output("Point")]
		public ISpread<ISpread<Vector3D>> Point;
		

		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			Point.SliceCount = SpreadMax;
			if(Stop[0]){
				return;	
			}
			
			if(Reset[0] || isInit){
				resetValue();
				isInit = false;
				return;
			}
			if(SpreadMax != LineCount[0]){
				changeNum();
			}
			
			if(Bang[0]){
				addValue(SpreadMax);
			}
			//Spread毎の処理
			for (int i = 0; i < posList.Count; i++)
			{	
				Vector3D sPos = StartPos[i];
				//FLogger.Log(LogType.Debug, "i:" + i);
				
				
				List<Vector3D> pList = 	posList[i];
				List<int> dList = 	dirList[i];
				if(pList.Count == MaxPointNum[i]){
					continue;
				}
				
				//Pointが0件の場合、リストに開始位置を追加
				Vector3D nextPos = new Vector3D();
				if(pList.Count == 0){
					pList.Add(sPos);
					//方向を決定
					int dirNum = getDifferentNumber(0);
					Vector3D dir = getDirection(dirNum);
					dList.Add(dirNum);
					//1辺の長さを設定
					unitLengthList.Add(0.1);
					//2点目を移動
					nextPos = sPos + dir * Speed[i];
					//FLogger.Log(LogType.Debug, "sPos:" + sPos.x + "," + sPos.y + "," + sPos.z);
					//FLogger.Log(LogType.Debug, "nextPos:" + nextPos.x + "," + nextPos.y + "," + nextPos.z);
					pList.Add(nextPos);
					//1辺の長さを設定
					unitLengthList.Add(0.1);
				}else if(pList.Count>0){
					//末尾を取得し延長
					Vector3D pos = (Vector3D)pList[pList.Count-1];
					int dirNum = (int)dList[dList.Count-1];
					Vector3D dir = getDirection(dirNum);
					pos = pos + dir * Speed[i];
					pList[pList.Count-1] = pos;
				}
				//末尾（描写中の線）の長さを確認
				Vector3D pos2 = (Vector3D)pList[pList.Count-1];
				Vector3D pos1 = (Vector3D)pList[pList.Count-2];
				
				double distance = (pos1 - pos2).Length;
				//FLogger.Log(LogType.Debug, "Length:" + distance);
				//2点目を決定
				if(distance > (double)unitLengthList[i]){
					//点を追加
					int newDirNum = getDifferentNumber(dList[dList.Count-1]);
					dList.Add(newDirNum);
					Vector3D newPos = pos2;
					pList.Add(newPos);
					//1辺の長さを設定
					unitLengthList.Add(cRandom.Next(100)/1000);
				}
				//Outputに設定
				Point[i].AssignFrom(pList);
				LeadPoint[i] = pList[pList.Count-1];
				Count[i] = pList.Count;
				
			}
		}
		
		private int getDifferentNumber(int nowNum){
			
			int ret = 0;
			int[] retValue0 = new int[]{2,3,4,5};
			int[] retValue1 = new int[]{0,1,4,5};
			int[] retValue2 = new int[]{0,1,2,3};
			int rValue = cRandom.Next(4);
			//FLogger.Log(LogType.Debug, "rValue:" + rValue);
			
			switch (nowNum)
			{
			    case 0:
					//2,3,4,5
					ret = retValue0[rValue];	
			        break;
			    case 1:
					//2,3,4,5
					ret = retValue0[rValue];	
			        break;
			    case 2:
					//0,1,4,5
					ret = retValue1[rValue];	
			        break;
			    case 3:
					//0,1,4,5
					ret = retValue1[rValue];	
			        break;
			    case 4:
					//0,1,2,3
					ret = retValue2[rValue];	
			        break;
			    case 5:
					//0,1,2,3
					ret = retValue2[rValue];	
			        break;
			    default:
					FLogger.Log(LogType.Debug, "Default");
			        break;
			}
			
			return ret;
		}
		
		private Vector3D getDirection(int direction){
			
			Vector3D ret = new Vector3D();
			//FLogger.Log(LogType.Debug, "direction:" + direction);
			switch (direction)
			{
			    case 0:
					ret.x = 1.0;
					ret.y = 0.0;
					ret.z = 0.0;
			        break;
			    case 1:
					ret.x = -1.0;
					ret.y = 0.0;
					ret.z = 0.0;
			        break;
			    case 2:
					ret.x = 0.0;
					ret.y = 1.0;
					ret.z = 0.0;
			        break;
			    case 3:
					ret.x = 0.0;
					ret.y = -1.0;
					ret.z = 0.0;
			        break;
			    case 4:
					ret.x = 0.0;
					ret.y = 0.0;
					ret.z = 1.0;
			        break;
			    case 5:
					ret.x = 0.0;
					ret.y = 0.0;
					ret.z = -1.0;
			        break;
			    default:
					FLogger.Log(LogType.Debug, "Default");
			        break;
			}
			
			return ret;
		}
		
		
		/**
		*
		*/
		private void addValue(int index){
			FLogger.Log(LogType.Debug, "addValue");
		
			FLogger.Log(LogType.Debug, "posList[index].SliceCount:" + posList[index].Count);
		}
		
		/**
		*
		*/
		private void resetValue(){
			FLogger.Log(LogType.Debug, "resetValue");
			init();

			
			
		
		}		
		
	}
}
