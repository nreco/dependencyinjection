using System;
using System.ComponentModel;
using System.Reflection;

namespace NReco.DependencyInjection {

	internal class DelegateFactory
	{
		public object TargetObject { get; set; }
		
		public string TargetMethod { get; set; }

		public Type DelegateType { get; set; }

		public DelegateFactory() {
		}

		public DelegateFactory(object o, string method) {
			TargetObject = o;
			TargetMethod = method;
		}		

		public object GetObject() {
			return Delegate.CreateDelegate(GetObjectType(), TargetObject, TargetMethod);
		}
		
		public Type GetObjectType() {
			if (DelegateType == null) {
				// autosuggest behaviour
				var targetType = TargetObject.GetType();
				var mInfo = targetType.GetMethod(TargetMethod);
				if (mInfo==null)
					throw new MissingMethodException(targetType.ToString(), TargetMethod );

				var mParams = mInfo.GetParameters();
				if (mInfo.ReturnType == typeof(void)) {
					var actionType = SuggestGenericType(actionTypeByParamCnt, mParams.Length);
					if (mParams.Length == 0) {
						return actionType;
					} else {
						var paramTypes = new Type[mParams.Length];
						for (int i = 0; i < paramTypes.Length; i++)
							paramTypes[i] = mParams[i].ParameterType;
						return actionType.MakeGenericType(paramTypes);
					}
				} else {
					var funcType = SuggestGenericType(funcTypeByParamCnt, mParams.Length);
					var paramTypes = new Type[mParams.Length+1];
					for (int i = 0; i < mParams.Length; i++)
						paramTypes[i] = mParams[i].ParameterType;
					paramTypes[ paramTypes.Length-1 ] = mInfo.ReturnType;
					return funcType.MakeGenericType(paramTypes);
				}
			}
			return DelegateType;
		}

		private static Type[] funcTypeByParamCnt = new[] {
			typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>), typeof(Func<,,,,>),typeof(Func<,,,,,>) 
		};
		private static Type[] actionTypeByParamCnt = new[] {
			typeof(Action), typeof(Action<>), typeof(Action<,>), typeof(Action<,,>), typeof(Action<,,,>),  typeof(Action<,,,,>) 
		};

		protected Type SuggestGenericType(Type[] types, int argsCnt) {
			if (argsCnt >= types.Length)
				throw new NotSupportedException("Too many arguments");
			return types[argsCnt];
		}
		
	}
}
