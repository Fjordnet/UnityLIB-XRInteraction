Shader "Fjord/InteractionHighlight" {
	Properties {
        _ColorL ("Color L", Color) = (1,1,1,1)
		_ColorR ("Color R", Color) = (1,1,1,1)
		
	    _SpreadL ("Spread L", float) = 4
        _SpreadR ("Spread R", float) = 4
	    	    
	    _RippleSize ("Ripple Size", float) = 200
		
	    _HighlightSourceL ("Highlight Source L", Vector) = (0,0,0,1)
		_HighlightSourceR ("Highlight Source R", Vector) = (0,0,0,1)
	}
	SubShader {	
		Tags { 
		    "RenderType" = "Transparent" 
            "Queue" = "Transparent+10"
        }
		LOD 200
		Offset -1, -1

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert alpha:fade

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

        half _RippleSize;
        half _SpreadL;
        half _SpreadR;
		half4 _HighlightSourceR;
		half4 _HighlightSourceL;
		fixed4 _ColorR;
		fixed4 _ColorL;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutput o) {			
			half distanceR = 1 - distance(_HighlightSourceR, IN.worldPos); 
		    half rippleR = clamp((sin(distanceR * _RippleSize) - .85) * 8, 0, 1);
			distanceR = clamp((distanceR * (_SpreadR + 1)) - _SpreadR, 0, 1);
			distanceR = (rippleR * distanceR) + clamp(pow(distanceR * 4, 1) - 3, 0, 1);
			
			half distanceL = 1 - distance(_HighlightSourceL, IN.worldPos); 
		    half rippleL = clamp((sin(distanceL * _RippleSize) - .85) * 8, 0, 1);
			distanceL = clamp((distanceL * (_SpreadL + 1)) - _SpreadL, 0, 1);
			distanceL = (rippleL * distanceL) + clamp(pow(distanceL * 4, 1) - 3, 0, 1);
				
		
			fixed4 cr = lerp(fixed4(_ColorR.x,_ColorR.y,_ColorR.z,0), _ColorR, distanceR);
			fixed4 cl = lerp(fixed4(_ColorL.x,_ColorL.y,_ColorL.z,0), _ColorL, distanceL);
			o.Albedo = (cr.rgb * cr.a) + (cl.rgb * cl.a);			
			o.Specular = 0;
			o.Gloss = 0;
			o.Alpha = cr.a + cl.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
