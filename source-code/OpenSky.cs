using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSky{
	
	public delegate double Equation(double a);
	public delegate Equation Linear(int[] x, double[] fx);

	public enum TimeType{
		DAY = 24, YEAR = 12
	}
	public class BaseTime{
		public static double velocity;
		public static Equation t = t => t*BaseTime.velocity;
		//Time
		public static Func<double,double> seconds = t => BaseTime.t(t)/1000; // quantos segundos se passaram
		public static Func<double,double> minutes = t => BaseTime.seconds(t)/60; // quantos minutos se passaram
		public static Func<double,double> hours = t => BaseTime.minutes (t) / 60; // quantas horas se passara
		public static Func<double,double> days = t => BaseTime.hours (t) / 24; //quantos dias se passaram

		public static Func<double,double> year = d => (d /365); //que ano é esse, o parametro é dia

		//Cycling Functions:
		public static Func<double,double> current_hours = t => 24*((BaseTime.t(t)/86400000.0)-(int)(BaseTime.t(t)/86400000.0)); //que hora é agora
		public static Func<double,double> current_minutes = t => 60*((BaseTime.t(t)/3600000.0)-(int)(BaseTime.t(t)/3600000.0)); //que hora é agora
		public static Func<double,double> current_secconds = t => 60*((BaseTime.t(t)/60000.0)-(int)(BaseTime.t(t)/60000.0)); //que segundo é agora
		public static Func<double,double> day = d => //que dia é hoje no ano
			d<=31?(int)d+1:
			d<=59?(int)d-31+1:
			d<=90?(int)d-59+1:
			d<=120?(int)d-90+1:
			d<=151?(int)d-120+1:
			d<=181?(int)d-151+1:
			d<=212?(int)d-181+1:
			d<=243?(int)d-212+1:
			d<=273?(int)d-243+1:
			d<=304?(int)d-273+1:
			d<=334?(int)d-304+1:
			d<=365?(int)d-334+1:
			BaseTime.day(d-365);
		public static Func<double,double> month = d => //que mes é esse no ano
			d<=31?1+d/31:
			d<=59?1+d/59*2:
			d<=90?1+d/90*3:
			d<=120?1+d/120*4:
			d<=151?1+d/151*5:
			d<=181?1+d/181*6:
			d<=212?1+d/212*7:
			d<=243?1+d/343*8:
			d<=273?1+d/273*9:
			d<=304?1+d/304*10:
			d<=334?1+d/334*11:
			d<=365?1+d/365*12:
			BaseTime.month(d-365);
	}
	public class Weather{

		//Main function of Interpolation
		public static Linear LinearSpline = (x, y) => num =>  num > 0?
																(y [(int)(num-1)] * 	((x [(int)(num-1) + 1] - num) / (x [(int)(num-1) + 1] - x [(int)(num-1)])))+ 
																(y [(int)(num-1) + 1] * ((num - x [(int)(num-1)]) 	  / (x [(int)(num-1) + 1] - x [(int)(num-1)]))):
																(y[0] + y[x.Length-1])/2;

		//essa função incrementa um copia do primeiro valor da array no fim da array
		private static double[] JoinEndToStart(double[] fx){
			double[] newFx = new double[fx.Length+1];
			for(int i = 0; i<fx.Length; i++){
				newFx [i] = fx [i];
			}
			newFx [fx.Length] = fx [0];
			return newFx;
		}
		//Interpolate the function on year time
		public static Equation InterpolationYear(double[] p_fx){
			double[] fx = Weather.JoinEndToStart (p_fx);
			int[] x = Enumerable.Range (1, fx.Length).ToArray<int> ();
			return t => Weather.LinearSpline (x, fx) (BaseTime.month (BaseTime.days (t)));
		}
		//Interpolate the function on day time
		public static Equation InterpolationDay(double[] p_fx){
			double[] fx = Weather.JoinEndToStart (p_fx);
			int[] x = Enumerable.Range(1,fx.Length).ToArray<int>();
			return t => Weather.LinearSpline (x, fx) (BaseTime.current_hours (t));
		}
		//Interpolate the function with a custom time function
		public static Equation InterpolationCustom(double[] p_fx, Func<double,double> baseTime){
			double[] fx = Weather.JoinEndToStart (p_fx);
			int[] x = Enumerable.Range(1,fx.Length).ToArray<int>();
			return t => Weather.LinearSpline (x, fx) (baseTime (t));
		}
		//Average about two functions
		public static Equation Average(Equation function1, Equation function2){
			return t => (function1 (t) / function2 (t)) / 2;
		}
		//Create a cyclic function based on Sin of hour
		public static Equation Simple(double mediam, double amplitude){
			return t => (Math.Sin ((BaseTime.t(t)/86400000 * Math.PI*2)+180) * amplitude) + mediam;
		}

	}

	public class Biome{

		private static double Patm = 0.94 * (Math.Pow (10, 5));
		private static double const_psicometer = 6.7 * (Math.Pow (10, -4));

		private static Equation tetens = temperature => 610.78*Math.Pow(10,((7.5*temperature)/(247.4+temperature)));
		private static Func<double,double,double> psicometro = (ts,tu) => Biome.tetens(tu) - Biome.const_psicometer*Biome.Patm*(ts-tu);
		private static Func<double,double,double> ferrel = (ts,tu) => Biome.tetens(tu) - 0.00066*(1+(0.00115*tu))*Biome.Patm*(ts-tu);

		//Construct
		public Biome(){
			
		}
		public Biome(string biome_type){
			if (biome_type == "basic") {
				this.temperature = Weather.Simple (25, 5);
				this.umidity = t => (Biome.ferrel (this.temperature (t), this.temperature (t)-7) / Biome.tetens (this.temperature (t)))*100;
			}
		}

		//Features
		public Equation temperature = t => 0;
		public Equation evaporation = t => 0;
		public Equation insolation = t => 0;
		public Equation pressure = t => 0;
		public Equation umidity = t => 0;
		public Equation rain = t=> 0;
		public Equation rainForce = t=> 0;
		public Equation fog = t=> 0;
		public Equation snow = t=> 0;

		//position
		public double x;
		public double y;
		public double z;
		public double extension;

		public void SetPosition(double x, double y, double z){
			this.x = x;
			this.y = y;
			this.z = z;
		}

		//distancia entre biomas
		public static double Distance(Biome biome1, Biome biome2){
			return Math.Sqrt (Math.Pow(biome1.x-biome2.x,2)+Math.Pow(biome1.y-biome2.y,2)+Math.Pow(biome1.z-biome2.z,2));
		}

	}
	public class BiomeManager{
		
		public struct Position{
			public double X;
			public double Y;
			public double Z;
		}

		public int potency;
		public Dictionary<string, Biome> biomes;
		public Position position;
		public double rayDistance;

		public BiomeManager(){
			position = new Position ();
			biomes = new Dictionary<string, Biome> ();
		}

		public void SetPosition(double x, double y, double z){
			this.position.X = x;
			this.position.Y = y;
			this.position.Z = z;
		}

		public void AddBiome(string name, Biome biome){
			biomes.Add (name, biome);
			potency = 3;
			rayDistance = 1000;
		}

		public double LocalTemperature(double time){
			double totoalSum = TotalInverseDistance ();
			if (biomes.Count > 0) {
				double sum = 0;
				foreach (KeyValuePair<string, Biome> biome in biomes) {
					if (DistanceFromBiome (biome.Value) < rayDistance) {
						sum += (biome.Value.temperature (time) * IDWFromBiome (biome.Value)) / totoalSum;
					}
				}
				return sum;
			} else {
				return 0;
			}
		}
		public double LocalEvaporation(double time){
			double totoalSum = TotalInverseDistance ();
			if (biomes.Count > 0) {
				double sum = 0;
				foreach (KeyValuePair<string, Biome> biome in biomes) {
					if (DistanceFromBiome (biome.Value) < rayDistance) {
						sum += (biome.Value.evaporation (time) * IDWFromBiome (biome.Value)) / totoalSum;
					}
				}
				return sum;
			} else {
				return 0;
			}
		}
		public double LocalInsolation(double time){
			double totoalSum = TotalInverseDistance ();
			if (biomes.Count > 0) {
				double sum = 0;
				foreach (KeyValuePair<string, Biome> biome in biomes) {
					if (DistanceFromBiome (biome.Value) < rayDistance) {
						sum += (biome.Value.insolation (time) * IDWFromBiome (biome.Value)) / totoalSum;
					}
				}
				return sum;
			} else {
				return 0;
			}
		}
		public double LocalPressure(double time){
			double totoalSum = TotalInverseDistance ();
			if (biomes.Count > 0) {
				double sum = 0;
				foreach (KeyValuePair<string, Biome> biome in biomes) {
					if (DistanceFromBiome (biome.Value) < rayDistance) {
						sum += (biome.Value.pressure (time) * IDWFromBiome (biome.Value)) / totoalSum;
					}
				}
				return sum;
			} else {
				return 0;
			}
		}
		public double LocalUmidity(double time){
			double totoalSum = TotalInverseDistance ();
			if (biomes.Count > 0) {
				double sum = 0;
				foreach (KeyValuePair<string, Biome> biome in biomes) {
					if (DistanceFromBiome (biome.Value) < rayDistance) {
						sum += (biome.Value.umidity (time) * IDWFromBiome (biome.Value)) / totoalSum;
					}
				}
				return sum;
			} else {
				return 0;
			}
		}
		public double LocalRain(double time){
			double totoalSum = TotalInverseDistance ();
			if (biomes.Count > 0) {
				double sum = 0;
				foreach (KeyValuePair<string, Biome> biome in biomes) {
					if (DistanceFromBiome (biome.Value) < rayDistance) {
						sum += (biome.Value.rain (time) * IDWFromBiome (biome.Value)) / totoalSum;
					}
				}
				return sum;
			} else {
				return 0;
			}
		}
		public double LocalRainForce(double time){
			double totoalSum = TotalInverseDistance ();
			if (biomes.Count > 0) {
				double sum = 0;
				foreach (KeyValuePair<string, Biome> biome in biomes) {
					if (DistanceFromBiome (biome.Value) < rayDistance) {
						sum += (biome.Value.rainForce (time) * IDWFromBiome (biome.Value)) / totoalSum;
					}
				}
				return sum;
			} else {
				return 0;
			}
		}
		public double LocalFog(double time){
			double totoalSum = TotalInverseDistance ();
			if (biomes.Count > 0) {
				double sum = 0;
				foreach (KeyValuePair<string, Biome> biome in biomes) {
					if (DistanceFromBiome (biome.Value) < rayDistance) {
						sum += (biome.Value.fog (time) * IDWFromBiome (biome.Value)) / totoalSum;
					}
				}
				return sum;
			} else {
				return 0;
			}
		}
		public double LocalSnow(double time){
			double totoalSum = TotalInverseDistance ();
			if (biomes.Count > 0) {
				double sum = 0;
				foreach (KeyValuePair<string, Biome> biome in biomes) {
					if (DistanceFromBiome (biome.Value) < rayDistance) {
						sum += (biome.Value.snow (time) * IDWFromBiome (biome.Value)) / totoalSum;
					}
				}
				return sum;
			} else {
				return 0;
			}
		}

		private double TotalInverseDistance(){
			if (biomes.Count > 0) {
				double totalSum = 0;
				foreach (KeyValuePair<string, Biome> biome in biomes) {
					if (DistanceFromBiome (biome.Value) < rayDistance) {
						totalSum += IDWFromBiome (biome.Value);
					}
				}
				return totalSum;
			} else {
				return 0;
			}
		}

		private double DistanceFromBiome(Biome biome){
			return Math.Sqrt (Math.Pow(biome.x-position.X,2)+Math.Pow(biome.y-position.Y,2)+Math.Pow(biome.z-position.Z,2));
		}

		private double IDWFromBiome(Biome biome){
			return 1 / Math.Pow((Math.Sqrt (Math.Pow (Math.Abs (biome.x - position.X), 2) +
				Math.Pow (Math.Abs (biome.y - position.Y), 2) +
				Math.Pow (Math.Abs (biome.z - position.Z), 2))), potency);
		}
	}
}
