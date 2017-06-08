
#version 430 core

#define MCM_GOOD_GRAPHICS 0
#define MCM_SHADOWS 0
#define MCM_SSAO 0

layout (binding = 1) uniform sampler2D positiontex; // The G-Buffer positions texture.
layout (binding = 2) uniform sampler2D normaltex; // The G-Buffer normals texture.
layout (binding = 3) uniform sampler2D depthtex; // The G-Buffer normals texture.
layout (binding = 4) uniform sampler2DArray shadowtex; // The shadow maps for the current lights.
layout (binding = 5) uniform sampler2D renderhinttex; // Rendering hint texture (x is specular strength, z is ambient light strength).
layout (binding = 6) uniform sampler2D diffusetex; // The diffuse texture (G-Buffer colors).

in struct vox_out // Represents data from the VS file.
{
	vec2 texcoord; // The texture coordinate.
} f; // It's named "f".

const int LIGHTS_MAX = 38; // How many lights we can ever have.

layout (location = 3) uniform float depth_jump = 0.5; // How much to jump around when calculating shadow coordinates.
layout (location = 4) uniform vec3 ambient = vec3(0.05); // How much ambient light to apply.
// ...
layout (location = 7) uniform vec2 zdist = vec2(0.1, 1000.0); // Z Near, Z Far.
layout (location = 8) uniform mat4 ssao_projection = mat4(1.0); // Current projection, for SSAO usage.
layout (location = 9) uniform float lights_used = 0.0; // How many lights are present.
layout (location = 10) uniform mat4 shadow_matrix_array[LIGHTS_MAX]; // The matrices of the light sources.
layout (location = 48) uniform mat4 light_data_array[LIGHTS_MAX]; // Data for all the lights.

const float HDR_Mod = 5.0; // The HDR modifier: multiply all lights by this constant to improve accuracy of colors.

out vec4 color; // The color to add to the lighting texture

float linearizeDepth(in float rinput) // Convert standard depth (stretched) to a linear distance (still from 0.0 to 1.0).
{
	return (2.0 * zdist.x) / (zdist.y + zdist.x - rinput * (zdist.y - zdist.x));
}

float fix_sqr(in float inTemp)
{
	return 1.0 - (inTemp * inTemp);
}

/*
GENERATION CODE (C#):
            Random random = new Random();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 32; i++)
            {
                double x = random.NextDouble() * 2.0 - 1.0;
                double y = random.NextDouble() * 2.0 - 1.0;
                double len = Math.Sqrt(x * x + y * y);
                sb.Append("vec2(" + (x / len) + ", " + (y / len) + "),\n");
            }
            File.WriteAllText("result.txt", sb.ToString());
*/
const vec2 noise[] = vec2[](
	vec2(0.816974284739033, -0.576674100402598),
	vec2(0.726527722388351, -0.687137154141147),
	vec2(-0.633311138034394, 0.773897281583015),
	vec2(-0.443630264109363, -0.896209902180431),
	vec2(0.925432831843231, 0.378911696502785),
	vec2(0.551369783597202, 0.834260967405268),
	vec2(0.357093278167287, -0.934068729102811),
	vec2(0.319371548819065, -0.947629576261163),
	vec2(-0.110968827538699, 0.993823887474379),
	vec2(0.516974131799351, 0.856001020472702),
	vec2(0.220619936417185, 0.975359853415793),
	vec2(-0.931004527808018, 0.365007628962696),
	vec2(-0.365920837144818, 0.930645980458324),
	vec2(0.994247847201261, 0.107103773676084),
	vec2(-0.812731166179419, -0.582638868872169),
	vec2(0.995906607516348, -0.0903882133094731),
	vec2(0.962474928685528, -0.271370616780423),
	vec2(-0.990446745041369, -0.137895776719076),
	vec2(0.88888643791026, -0.458127602856681),
	vec2(0.741624968602105, -0.670814732952345),
	vec2(0.809174192721238, 0.587568826465405),
	vec2(-0.912090905233409, -0.409988024935485),
	vec2(-0.87259311134526, -0.48844780891391),
	vec2(0.479846950737638, 0.877352211980907),
	vec2(0.607808857406633, 0.794083366440856),
	vec2(-0.338430704197075, 0.940991316886969),
	vec2(0.226678350490074, 0.973969673767669),
	vec2(0.682856659211184, -0.730552382085597),
	vec2(-0.897770025920797, -0.440464505446434),
	vec2(0.655363709131766, -0.755313450663401),
	vec2(-0.64597373980502, 0.763359631813418),
	vec2(0.998062361689854, -0.0622215571813358)
);

/*
GENERATION CODE(C#):

            Random random = nw Random();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 64; i++)
            {
                double x = random.NextDouble() * 2.0 - 1.0;
                double y = random.NextDouble() * 2.0 - 1.0;
                double z = random.NextDouble();
                double len = Math.Sqrt(x * x + y * y + z * z);
                sb.Append("vec3(" + (x / len) + ", " + (y / len) + ", " + (z / len) + "),\n");
            }
            File.WriteAllText("result.txt", sb.ToString());
*/
const vec3 kernel[] = vec3[](
	vec3(0.50523790717783, 0.638495093231913, 0.580567543933811),
	vec3(0.591922892679008, -0.0393588399305392, 0.805033024690191),
	vec3(-0.0269829191557532, 0.806235548218982, 0.59097898681922),
	vec3(-0.749335223414985, 0.350483386515607, 0.561834600862359),
	vec3(0.805247035459988, -0.218806569862496, 0.55109064305966),
	vec3(-0.317110752787967, 0.919712533761122, 0.231451130282283),
	vec3(0.663635679797808, 0.468138701855306, 0.58346708589648),
	vec3(-0.17992278720326, 0.0804220867970362, 0.980387718507438),
	vec3(0.202675089221641, -0.87670512272559, 0.436246416599274),
	vec3(0.37757821445279, -0.586869211202515, 0.716253600977461),
	vec3(-0.889925291076573, 0.300957513395599, 0.34272080682826),
	vec3(-0.147704495244744, -0.440612951222886, 0.885462370346225),
	vec3(0.865252867579653, -0.0137069085989239, 0.501148277261179),
	vec3(-0.529711815582452, 0.833450303389426, 0.157372120187839),
	vec3(-0.564959327589597, -0.179847464138838, 0.805279981008059),
	vec3(0.497943313749438, 0.639742798749804, 0.585475539830655),
	vec3(0.189072239743947, 0.611038026357689, 0.768689936517389),
	vec3(-0.534489745280417, 0.736504836522392, 0.41458574260242),
	vec3(-0.870493063648821, -0.0863019146101891, 0.48455526586129),
	vec3(-0.662022530327602, -0.695014464956345, 0.280501448909066),
	vec3(-0.067887160253829, 0.712081049665431, 0.698807492933532),
	vec3(0.809837343882758, 0.576391976045415, 0.109250933192271),
	vec3(-0.365901548118161, 0.0114901260686164, 0.930582631520522),
	vec3(0.339857468567267, -0.403445530075345, 0.849546117242186),
	vec3(0.406544571037622, 0.29297426698646, 0.865382915618042),
	vec3(0.49098514549523, -0.85232403152985, 0.180214683585247),
	vec3(0.785505315730742, 0.507278963699669, 0.354470664437182),
	vec3(0.0325130731131695, -0.91143939203003, 0.410147698680205),
	vec3(0.380229451685504, 0.68164587542891, 0.625127558648389),
	vec3(0.664499021198278, -0.274883163411749, 0.694895889539779),
	vec3(0.0107509418865127, 0.846902019327788, 0.531640279613075),
	vec3(-0.567721851169424, 0.822970414553638, 0.0202878405498193),
	vec3(-0.0234283738202667, -0.615716742292373, 0.78761920022369),
	vec3(0.586357088509774, -0.506857716621407, 0.631886556159945),
	vec3(0.196002449853212, -0.886491221211885, 0.419185346077127),
	vec3(-0.831265689142906, 0.365610233135833, 0.418720087266102),
	vec3(-0.704159643832156, -0.41047940375318, 0.579366770787388),
	vec3(-0.465293997405335, -0.502660397665097, 0.728583571457476),
	vec3(0.552382532148862, 0.635406508241915, 0.539566592239206),
	vec3(-0.444021297897998, -0.498047966610255, 0.744844487103427),
	vec3(-0.825585279789057, 0.427895551425575, 0.367850979155728),
	vec3(-0.313894302561715, -0.737422015797692, 0.598062820643586),
	vec3(-0.552761080495518, -0.103988775577333, 0.826826174261165),
	vec3(0.623025069317336, -0.445913654382964, 0.642651364144634),
	vec3(0.0738222610921289, -0.831313104756351, 0.550880019266992),
	vec3(0.734195594997187, 0.677758042864879, 0.0400107937773642),
	vec3(0.884609956220488, -0.261371634669868, 0.386199551974867),
	vec3(0.343401584411166, 0.0544684486782016, 0.937607881751477),
	vec3(0.152621797092288, -0.864705167030252, 0.478530627194858),
	vec3(0.589615543897804, -0.201666649782541, 0.782102341614947),
	vec3(-0.71611924815832, -0.692096453021359, 0.0904196999133216),
	vec3(0.464374466413936, -0.301491682272031, 0.832741929089411),
	vec3(-0.453421566341103, -0.387673257650842, 0.802569827790183),
	vec3(0.668400548429906, -0.734478886078198, 0.117394517605934),
	vec3(-0.806421658704416, -0.385546487098633, 0.448372629247506),
	vec3(-0.538595551360479, 0.821711110813931, 0.186294611891051),
	vec3(-0.700220511420765, 0.712997016918229, 0.0364210001420711),
	vec3(0.433274981814638, -0.385996269642977, 0.814419836420523),
	vec3(0.0248614781788934, 0.977242817870262, 0.210661771149279),
	vec3(-0.441040550660678, 0.440650562668534, 0.781863360372406),
	vec3(-0.900950545362814, -0.419549668078104, 0.110752836649903),
	vec3(0.695041214521007, 0.305860345361649, 0.650666703660497),
	vec3(0.59493702123171, 0.49059534729015, 0.636683709533389),
	vec3(0.396957020675991, 0.489798609358311, 0.776223193422295)
);

const float ssao_radius = 0.025; // TODO: Uniform?

float ssao_color(in vec3 position, in vec3 normal, vec3 difcol)
{
	float pos_zm = linearizeDepth(texture(depthtex, f.texcoord).x);
	vec3 rv = vec3(noise[int(dot(position, position) + dot(difcol, difcol) * 17.32 + pos_zm * 72.8) % 32], 0.0);
	vec3 tangent = normalize(rv - normal * dot(rv, normal));
	vec3 bitangent = cross(normal, tangent);
	mat3 tbn = mat3(tangent, bitangent, normal);
	float occlusion = 0.0;
	for (int i = 0; i < 64; i++) {
		vec3 vsample = tbn * kernel[i];
		vsample = vsample * ssao_radius + position;
		vec4 offset = ssao_projection * vec4(vsample, 1.0);
		offset.xyz /= offset.w;
		offset.xyz = offset.xyz * 0.5 + 0.5;
		float sampleDepth = linearizeDepth(texture(depthtex, offset.xy).x);
		float rangeCheck = abs(pos_zm - sampleDepth) < ssao_radius ? 1.0 : 0.0;
		occlusion += (sampleDepth <= linearizeDepth(offset.z) ? 1.0 : 0.0) * rangeCheck;
	}
	return 1.0 - (occlusion * (0.5 / 64.0));
}

void main() // Let's put all code in main, why not...
{
	//color = vec4(texture(shadowtex, vec3(f.texcoord.xy, 1.0)).x, 0.0, 0.0, 1.0); return; // ------------------------------------------
	vec3 res_color = vec3(0.0);
	float aff = 0.0;
	// Gather all the texture information.
	vec3 normal = texture(normaltex, f.texcoord).xyz;
	vec3 position = texture(positiontex, f.texcoord).xyz;
	vec3 renderhint = texture(renderhinttex, f.texcoord).xyz;
	vec4 diffuset = texture(diffusetex, f.texcoord);
#if MCM_SSAO
	float ssao_mod = 1.0;
	if (renderhint.z < 1.0)
	{
		ssao_mod = (ssao_color(position, normal, diffuset.xyz) * (1.0 - renderhint.z) * 0.9) + 0.1;
	}
#else
	const float ssao_mod = 1.0;
#endif
	vec3 N = -normal;
	// Loop over lights
	int count = renderhint.z >= 1.0 ? 0 : int(lights_used);
	for (int i = 0; i < count; i++)
	{
	mat4 light_data = light_data_array[i];
	mat4 shadow_matrix = shadow_matrix_array[i];
	// Light data.
	vec3 light_pos = vec3(light_data[0][0], light_data[0][1], light_data[0][2]); // The position of the light source.
	float diffuse_albedo = light_data[0][3]; // The diffuse albedo of this light (diffuse light is multiplied directly by this).
	float specular_albedo = light_data[1][0]; // The specular albedo (specular power is multiplied directly by this).
	float should_sqrt = light_data[1][1]; // 0 to not use square-root trick, 1 to use it (see implementation for details).
	vec3 light_color = vec3(light_data[1][2], light_data[1][3], light_data[2][0]); // The color of the light.
	float light_radius = light_data[2][1]; // The maximum radius of the light.
	vec3 eye_pos = vec3(light_data[2][2], light_data[2][3], light_data[3][0]); // The position of the camera eye.
	float light_type = light_data[3][1]; // What type of light this is: 0 is standard (point, sky, etc.), 1 is conical (spot light).
	float tex_size = light_data[3][2]; // If shadows are enabled, this is the inverse of the texture size of the shadow map.
	// float unused = light_data[3][3];
	vec4 f_spos = shadow_matrix * vec4(position, 1.0); // Calculate the position of the light relative to the view.
	f_spos /= f_spos.w; // Standard perspective divide.
	vec3 light_path = light_pos - position; // What path a light ray has to travel down in theory to get from the source to the current pixel.
	float light_length = length(light_path); // How far the light is from this pixel.
	float d = light_length / light_radius; // How far the pixel is from the end of the light.
	float atten = clamp(1.0 - (d * d), 0.0, 1.0); // How weak the light is here, based purely on distance so far.
	if (light_type >= 0.5) // If this is a conical (spot light)...
	{
		atten *= 1.0 - (f_spos.x * f_spos.x + f_spos.y * f_spos.y); // Weaken the light based on how far towards the edge of the cone/circle it is. Bright in the center, dark in the corners.
	}
	if (atten <= 0.0) // If light is really weak...
	{
		continue; // Forget this light, move on already!
	}
	if (should_sqrt >= 0.5) // If inverse square trick is enabled (generally this will be 1.0 or 0.0)
	{
		f_spos.x = sign(f_spos.x) * fix_sqr(1.0 - abs(f_spos.x)); // Inverse square the relative position while preserving the sign. Shadow creation buffer also did this.
		f_spos.y = sign(f_spos.y) * fix_sqr(1.0 - abs(f_spos.y)); // This section means that coordinates near the center of the light view will have more pixels per area available than coordinates far from the center.
	}
	// Create a variable representing the proper screen/texture coordinate of the shadow view (ranging from 0 to 1 instead of -1 to 1).
	vec3 fs = f_spos.xyz * 0.5 + vec3(0.5, 0.5, 0.5); 
	if (fs.x < 0.0 || fs.x > 1.0
		|| fs.y < 0.0 || fs.y > 1.0
		|| fs.z < 0.0 || fs.z > 1.0) // If any coordinate is outside view range...
	{
		continue; // We can't light it! Discard straight away!
	}
	// This block only runs if shadows are enabled.
#if MCM_SHADOWS
	// Pretty quality (soft) shadows require a quality graphics card.
#if MCM_GOOD_GRAPHICS
	// This area is some calculus-ish stuff based upon NVidia sample code (naturally, it seems to run poorly on AMD cards. Good area to recode/optimize!)
	// It's used to take the shadow map coordinates, and gets a safe Z-modifier value (See below).
	vec3 duvdist_dx = dFdx(fs);
	vec3 duvdist_dy = dFdy(fs);
	vec2 dz_duv = vec2(duvdist_dy.y * duvdist_dx.z - duvdist_dx.y * duvdist_dy.z, duvdist_dx.x * duvdist_dy.z - duvdist_dy.x * duvdist_dx.z);
	float tlen = (duvdist_dx.x * duvdist_dy.y) - (duvdist_dx.y * duvdist_dy.x);
	dz_duv /= tlen;
	float oneoverdj = 1.0 / depth_jump;
	float jump = tex_size * depth_jump;
	float depth = 0;
	float depth_count = 0;
	// Loop over an area quite near the pixel on the shadow map, but still covering multiple pixels of the shadow map.
	for (float x = -oneoverdj * 2; x < oneoverdj * 2 + 1; x++)
	{
		for (float y = -oneoverdj * 2; y < oneoverdj * 2 + 1; y++)
		{
			float offz = dot(dz_duv, vec2(x * jump, y * jump)) * 1000.0; // Use the calculus magic from before to get a safe Z-modifier.
			if (offz > -0.000001) // (Potentially removable?) It MUST be negative, and below a certain threshold. If it's not...
			{
				offz = -0.000001; // Force it to the threshold value to reduce errors.
			}
			offz -= 0.001; // Set it a bit farther regardless to reduce bad shadows.
			float rd = texture(shadowtex, vec3(fs.x + x * jump, fs.y + y * jump, float(i))).r; // Calculate the depth of the pixel.
			depth += (rd >= (fs.z + offz) ? 1.0 : 0.0); // Get a 1 or 0 depth value for the current pixel. 0 means don't light, 1 means light.
			depth_count++; // Can probably use math to generate this number rather than constantly incrementing a counter.
		}
	}
	depth = depth / depth_count; // Average up the 0 and 1 light values to produce gray near the edges of shadows. Soft shadows, hooray!
#else // Good Graphics
	float rd = texture(shadowtex, vec3(fs.x, fs.y, float(i))).r; // Calculate the depth of the pixel.
	float depth = (rd >= (fs.z - 0.001) ? 1.0 : 0.0); // If we have a bad graphics card, just quickly get a 0 or 1 depth value. This will be pixelated (hard) shadows!
#endif // Else - Good Graphics
	if (depth <= 0.0)
	{
		continue; // If we're a fully shadowed pixel, don't add any light!
	}
#else // Shadows
	const float depth = 1.0; // If shadows are off, depth is a constant 1.0!
#endif // Else - Shadows
	vec3 L = light_path / light_length; // Get the light's movement direction as a vector
	vec3 diffuse = max(dot(N, -L), 0.0) * vec3(diffuse_albedo) * HDR_Mod; // Find out how much diffuse light to apply
	vec3 specular = vec3(pow(max(dot(reflect(L, N), normalize(position - eye_pos)), 0.0), 200.0) * specular_albedo * renderhint.x) * HDR_Mod; // Find out how much specular light to apply.
	res_color += (vec3(depth, depth, depth) * atten * (diffuse * light_color) * diffuset.xyz) + (min(specular, 1.0) * light_color * atten * depth); // Put it all together now.
	aff += atten;
	}
	res_color += (ambient + vec3(renderhint.z)) * HDR_Mod * diffuset.xyz; // Add ambient light.
	color = vec4(res_color * ssao_mod, diffuset.w + aff + renderhint.z); // I don't know why this alpha value became necessary.
}
