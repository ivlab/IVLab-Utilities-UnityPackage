Shader "Unlit/AlwaysOnTop"
{
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
	}

	SubShader{
		Tags {
			"RenderType" = "Opaque"
			"Queue" = "Transparent+2"  // Volumes are rendered +1, so render this +2
		}

		LOD 100

		// Render on top of all other objects by always passing z test
		ZTest Always

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
			};

			fixed4 _Color;

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				fixed4 col = _Color;
				return col;
			}
		ENDCG
		}
	}
}
