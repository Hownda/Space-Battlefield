Shader "Hidden/Atmosphere"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PlanetCenter("Planet Center", Vector) = (0, 0, 0, 0)
        _PlanetRadius("Planet Radius", float) = 10
        _AtmosphereRadius("Atmosphere Radius", float) = 20
        dirToSun("Direction To Sun", Vector) = (0, 0, -1)
        densityFalloff("Density Falloff", float) = 1
        numInScatteringPoints("Num In Scattering Points", int) = 1
        numOpticalDepthPoints("Num Optical Depth Points", int) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent-499"}

        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "UnityCG.cginc"
            #include "Math.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 viewVector : TEXCOORD1;
			};

            v2f vert (appdata v) {
				v2f output;
				output.pos = UnityObjectToClipPos(v.vertex);
				output.uv = v.uv;
				// Camera space matches OpenGL convention where cam forward is -z. In unity forward is positive z.
				// (https://docs.unity3d.com/ScriptReference/Camera-cameraToWorldMatrix.html)
				float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv.xy * 2 - 1, 0, -1));
				output.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));
				return output;
			}

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float3 _PlanetCenter;
            float _PlanetRadius;
            float _AtmosphereRadius;

            float3 dirToSun;
            float3 waveLengths;
            float densityFalloff;
            int numInScatteringPoints;
            int numOpticalDepthPoints;
            float3 scatteringCoefficients;
            

            float densityAtPoint(float3 densitySamplePoint)
            {
                float heightAboveSurface = length(densitySamplePoint - _PlanetCenter) - _PlanetRadius;
                float height01 = heightAboveSurface / (_AtmosphereRadius - _PlanetRadius);
                float localDensity = exp(-height01 * densityFalloff) * (1 - height01);
                return localDensity;
            }

            float opticalDepth(float3 rayOrigin, float3 rayDir, float rayLength)
            {
                float3 densitySamplePoint = rayOrigin;
                float stepSize = rayLength / (numOpticalDepthPoints - 1);
                float opticalDepth = 0;

                for (int i = 0; i < numOpticalDepthPoints; i++)
                {
                    float localDensity = densityAtPoint(densitySamplePoint);
                    opticalDepth += localDensity * stepSize;
                    densitySamplePoint += rayDir * stepSize;
                }
                return opticalDepth;
            }

            float3 calculateLight(float3 rayOrigin, float3 rayDir, float rayLength, float3 originalCol, float2 uv)
            {
                float3 inScatterPoint = rayOrigin;
                float stepSize = rayLength / (numInScatteringPoints - 1);
                float3 inScatteredLight = 0;
                float viewRayOpticalDepth = 0;

                for (int i = 0; i < numInScatteringPoints; i++)
                {
                    float sunRayLength = raySphere(_PlanetCenter, _AtmosphereRadius, inScatterPoint, dirToSun).y;
                    float sunRayOpticalDepth = opticalDepth(inScatterPoint, dirToSun, sunRayLength);
                    float3 transmittance = exp(-(sunRayOpticalDepth + viewRayOpticalDepth) * scatteringCoefficients);
                    float localDensity = densityAtPoint(inScatterPoint);

                    inScatteredLight += localDensity * transmittance * scatteringCoefficients * stepSize;
                    inScatterPoint += rayDir * stepSize;
                }
                float originalColTransmittance = exp(-viewRayOpticalDepth);
                return originalCol * originalColTransmittance + inScatteredLight;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 originalCol = tex2D(_MainTex, i.uv);
                float sceneDepthNonLinear  = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                float sceneDepth = LinearEyeDepth(sceneDepthNonLinear) * length(i.viewVector);

                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = normalize(i.viewVector);

                float2 hitInfo = raySphere(_PlanetCenter, _AtmosphereRadius, rayOrigin, rayDir);
                float dstToAtmosphere = hitInfo.x;
                float dstThroughAtmosphere = min(hitInfo.y, sceneDepth - dstToAtmosphere);

                if (dstThroughAtmosphere > 0)
                {
                    const float epsilon = 0.0001;
                    float3 pointInAtmosphere = rayOrigin + rayDir * dstToAtmosphere;
                    float3 light = calculateLight(pointInAtmosphere, rayDir, dstThroughAtmosphere - epsilon * 2, originalCol, i.uv);
                    return float4(light, 1);
                }
                return originalCol;
            }
            ENDCG
        }
    }
}
