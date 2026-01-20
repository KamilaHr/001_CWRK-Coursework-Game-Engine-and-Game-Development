#version 330 core

in vec3 vDir;
out vec4 FragColor;

void main()
{
    vec3 dir = normalize(vDir);

    float t = normalize(vDir).y * 0.5 + 0.5;
    vec3 top vec3(0.10, 0.15,0.30);
    vec3 bottom = vec3(0.02, 0.02, 0.05);
    vec3 color = mix(bottom, top, t);

    vec3 sunDirection = normalize(vec3(1.0, 0.2, 0.0));
    float sunAmount = max(dot(dir, sunDirection), 0.0);
    float _sun = pow(sunAmount, 200.0);
    color += vec3(1.0, 0.9, 0.6) * _sun * 3.0;

    FragColor = vec4(color, 1.0);
}