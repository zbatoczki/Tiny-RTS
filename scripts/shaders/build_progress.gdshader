shader_type canvas_item;

// 0.0 = just placed (fully ghosted), 1.0 = finished (fully opaque)
uniform float progress : hint_range(0.0, 1.0) = 1.0;

// Alpha of the in-progress portion
uniform float ghost_alpha : hint_range(0.0, 1.0) = 0.35;

// Softness of the build line (in UV space). 0.0 = hard edge.
uniform float edge_softness : hint_range(0.0, 0.5) = 0.00;

void fragment() {
	vec4 original_texture = texture(TEXTURE, UV);

	// UV.y == 1.0 is the bottom of the sprite, so the built region
	// grows upward as progress increases.
	float build_line = 1.0 - progress;

	// 0 above the line (unbuilt) -> 1 below the line (built), with a soft edge.
	float built = smoothstep(build_line - edge_softness, build_line + edge_softness, UV.y);

	float alpha = mix(ghost_alpha, 1.0, built);

	COLOR = vec4(original_texture.rgb, original_texture.a * alpha);
}
