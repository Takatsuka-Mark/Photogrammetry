## Alternate Method 1

https://imagemagick.org/Usage/lens/correcting_lens_distortions.pdf

Real world coords, X, Y
Image coords, x,y

x_hat, y_hat denote image coordinates of the idea perspective mapping.

We need relationship between physical coordinates of pixel, x,y
    and coordinates x_hat, y_hat of ideal perspective


Now, let x_hat, y_hat to refer to principal point and x,y to refer to physical centre of image.
Offset of principal point being x_hat_p, y_hat_p.

Split lens distortion into radial and tangential part

r_hat^2 = x_hat^2 + y_hat^2
r^2 = (x - x_hat_p) ^ 2 + (y - y_hat_p) ^ 2
r_hat = f(r) * r
r_hat * alpha = g(r, alpha) * r * alpha

barrel distortion, f(r) < 1
pincussion distortion f(r) > 1

### This is tunable with `an` and different functions are possible
f(r) = 1 + a1 * r + a2 * r ^ 2 + a3 * r ^ 3

incorporating tangential distortion
(x_hat, y_hat) = (f) * (x, y) + (2 * a4 * x * y + a5 * (r^2 + 2 * x^2), a4 * (r^2 + 2y^2) + 2 * a5 * x * y)


## Alternate Method 2

https://iopscience.iop.org/article/10.1088/1742-6596/1453/1/012136/pdf
Only vers radial distortion. tangential distortion "can be ignored for most computer vision applications"


## Alternate Method 3

https://www.ncbi.nlm.nih.gov/pmc/articles/PMC8624224/
let (x0, y0) be the position where light is imaged on the camera sensor when there is no distortion
let (x, y) be the actual position after the distortion occurs.

x0 = x(1 + k1 * r^2 + k2 r^4 + k3 * r^6)
y0 = y(1 + k1 * r^2 + k2 r^4 + k3 * r^6)

## Alternate Method 4

> Note this one seems the most promising alternative
https://www.researchgate.net/publication/269271727_A_fisheye_distortion_correction_algorithm_optimized_for_hardware_implementations

uv plane is selected ROI
xy plane represents the image sensor
p is arbitrary point on uv plane

p(uv) = (pu, pv)
p(xyz) = (px, py, pz)

Transforming unit vectors in the uv plane, then using the linear relationship
p(xyz) = p'(xyz) + (p(xyz) - p'(xyz))
    = p'(xyz) + pu u^ (xyz) + pv v^ (xyz)
u^ and v^ are unit vectors in the uv plane.
p' is a vector pointing at the center of the ROI with length equal to mR

Unit vectors
Can be calculated through sequence of 2D vector rotations

Let u^ (uv) = (1, 0) be one unit vector.
(k)angle_alpha denote a rotation of a 2D vector k by angle alpha
It's representation in xyz frame is 
a = (u^ (uv))angle_psi
b = (a1, 0)angle_negative_beta
c = (b1, a2)angle_gamma
u^ (xyz) = (c1, c2, b2)
where a,b,c are vectors resulting from intermediate computations.

Repeat procedure to get v^

Next step is rectangular to polar coordinates

gammap = arctan(py / px)
betap = arctan(pxy / pz)

pxy = sqrt((px)^2 + (py)^2)
betap is sensor incidence angle
If orthogonal fisheye lens was used, (px,py) givese exact location of pixel in the image acquired by fisheye.

In general case, lens function needs to be included.
So, map with lens function
(pfx, pfy) given by

rp = R * Flens(Betap)
pfx = rp * cos(gammap)
pfy = rp * sin(gammap)

