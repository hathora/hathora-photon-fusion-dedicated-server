// Created by dylan@hathora.dev

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Hathora.Core.Scripts.Runtime.Common.Extensions;
using HathoraCloud;
using HathoraCloud.Models.Operations;
using HathoraCloud.Models.Shared;
using UnityEngine;

namespace HathoraPhoton
{
	/// <summary>
	/// To prevent confusion, maps do not override Photon's region whitelist.
	/// Instead, this is more so useful for *dynamically creating* a game.
	/// </summary>
    public static class HathoraRegionUtility
	{
		/// <summary>
		///  Learn more about Photon Cloud regions here: https://doc.photonengine.com/fusion/current/manual/connection-and-matchmaking/regions
		/// </summary>
		private static readonly Dictionary<string, Region> _photonToHathora = new Dictionary<string, Region>()
		{
			{ "us",   Region.WashingtonDC },
			{ "usw",  Region.Seattle      },
			{ "asia", Region.Singapore    },
			{ "jp",   Region.Tokyo        },
			{ "eu",   Region.Frankfurt    },
			{ "sa",   Region.SaoPaulo     },
			{ "in",   Region.Mumbai       },
			{ "kr",   Region.Tokyo        },
		};

		/// <summary>
		///  Learn more about Hathora Cloud regions here: https://hathora.dev/docs/faq/scale-globally
		/// </summary>
		private static readonly Dictionary<Region, string> _hathoraToPhoton = new Dictionary<Region, string>()
		{
			{ Region.Seattle,      "usw"  },
			{ Region.LosAngeles,   "usw"  },
			{ Region.WashingtonDC, "us"   },
			{ Region.Chicago,      "us"   },
			{ Region.London,       "eu"   },
			{ Region.Frankfurt,    "eu"   },
			{ Region.Mumbai,       "in"   },
			{ Region.Singapore,    "asia" },
			{ Region.Tokyo,        "jp"   },
			{ Region.Sydney,       "jp"   },
			{ Region.SaoPaulo,     "sa"   },
		};

		/// <summary>
		/// Convert Photon region string to Hathora Region
		/// Example usage: `Region hathoraRegion = HathoraRegionUtility.PhotonToHathora(_sessionRegion);`
		/// </summary>
		public static Region PhotonToHathora(string photonRegion)
		{
			return _photonToHathora[photonRegion];
		}

		/// <summary>
		/// Convert Hathora Region to Photon region string
		/// Example usage: `string _sessionRegion = HathoraRegionUtility.HathoraToPhoton(hathoraRegion);`
		/// </summary>
		public static string HathoraToPhoton(Region hathoraRegion)
		{
			return _hathoraToPhoton[hathoraRegion];
		}

		/// <summary>
		/// Utility method to help determine the lowest ping for a Hathora region (useful when player is requesting a match to be created)
		/// </summary>
		public static async Task<(bool bestRegionFound, Region bestRegion, double bestRegionPing)> FindBestRegion(HathoraCloudSDK hathoraCloudSDK, Region fallbackRegion, bool enableLogs = false)
		{
			// 1. Get all Hathora endpoints.
			GetPingServiceEndpointsResponse pingEndpointsResponse = await hathoraCloudSDK.DiscoveryV1.GetPingServiceEndpointsAsync();
			if (pingEndpointsResponse.DiscoveryResponse == null)
				return (false, fallbackRegion, default);

			// 2. Create ping request.
			List<Tuple<Region, List<Ping>>> regionPings = new List<Tuple<Region, List<Ping>>>();
			foreach (DiscoveryResponse endpoint in pingEndpointsResponse.DiscoveryResponse)
			{
				string ip = TryGetIPAddress(endpoint.Host);

				if (enableLogs == true)
				{
					Debug.Log($"Endpoint Region: {endpoint.Region}   Host: {endpoint.Host}   Port: {endpoint.Port}   IP: {ip}");
				}

				// 6 pings for each endpoint, then calculating average ping.
				List<Ping> pings = new()
				{
					new Ping(ip),
					new Ping(ip),
					new Ping(ip),
					new Ping(ip),
					new Ping(ip),
					new Ping(ip)
				};
				regionPings.Add(new Tuple<Region, List<Ping>>(endpoint.Region, pings));
			}

			// 3. Wait for ping responses.
			const int delay = 20;
			const int iterations = 50;
			for (int i = 0; i < iterations; ++i)
			{
				await Task.Delay(delay);

				bool allPingsDone = true;

				foreach(Tuple<Region, List<Ping>> regionPing in regionPings)
				{
					foreach (Ping ping in regionPing.Item2)
					{
						allPingsDone &= ping.isDone;
					}
				}

				if (allPingsDone == true)
				{
					if (enableLogs == true)
					{
						Debug.LogWarning($"All pings done after ~{i * delay}ms");
					}

					break;
				}
			}

			// 4. Find best region with lowest ping response.
			Region bestRegion      = fallbackRegion;
			double bestRegionPing  = Double.MaxValue;
			bool   bestRegionFound = false;

			foreach(Tuple<Region, List<Ping>> regionPing in regionPings)
			{
				double pingTime  = 0.0;
				int    pingCount = 0;

				foreach (Ping ping in regionPing.Item2)
				{
					if (ping.isDone == true && ping.time > 0)
					{
						if (enableLogs == true)
						{
							Debug.Log($"Region: {regionPing.Item1}   IP: {ping.ip}   Ping: {ping.time}ms");
						}

						pingCount++;
						pingTime += ping.time;
					}
					else
					{
						if (enableLogs == true)
						{
							Debug.LogWarning($"Region: {regionPing.Item1}   IP: {ping.ip}   Ping: ---ms");
						}
					}
				}

				if (pingCount > 0)
				{
					double averageRegionPing = pingTime / pingCount;
					if (averageRegionPing < bestRegionPing)
					{
						bestRegion      = regionPing.Item1;
						bestRegionPing  = averageRegionPing;
						bestRegionFound = true;
					}
				}
			}

			return (bestRegionFound, bestRegion, bestRegionPing);
		}

		private static string TryGetIPAddress(string hostname)
		{
			IPHostEntry host = Dns.GetHostEntry(hostname);

			foreach (IPAddress ip in host.AddressList)
			{
				if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
					return ip.ToString();
			}

			return hostname;
		}
		
		/// <summary>
		/// Eg: "WashingtonDC" -> "Washington DC". Useful for UI.
		/// </summary>
		public static string GetFriendlyRegionName(string region)
		{
			return region.SplitPascalCase();
		}
	}
}
