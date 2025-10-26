Shader "Graph/Point Surface GPU" {

    Properties {
        _Smoothness ("Smoothness", Range(0,1)) = .5
    }

    SubShader {
        CGPROGRAM
        #pragma surface ConfigureSurface Standard fullforwardshadows addshadow
        #pragma instancing_options procedural:ConfigureProcedural
        #pragma target 4.5

        float _Smoothness;

        struct Input{
            float3 worldPos;
        };

        struct Point{
            float2 position;
            float2 velocity;
            float radius;
        };

        #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
        StructuredBuffer<Point> points;
        #endif

        void ConfigureProcedural() {
            #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
                float2 position = points[unity_InstanceID].position;
                unity_ObjectToWorld = 0.0;
				unity_ObjectToWorld._m03_m13_m23_m33 = float4(position, 0.0, 1.0) * 5.f;
                unity_ObjectToWorld._m00_m11_m22 = 1.f;
			#endif
        }

        void ConfigureSurface(Input input, inout SurfaceOutputStandard surface){
            surface.Albedo = input.worldPos * .5f + .5f;
            surface.Smoothness = _Smoothness;
        }

        ENDCG
    }

    Fallback "Diffuse"
}