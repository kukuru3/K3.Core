float2 hexround(float2 pixel) {
    
    float q = round(pixel.x);
    float r = round(pixel.y);
    float initS = - pixel.x - pixel.y;

    float s = round(initS);

    float qdiff = abs(q - pixel.x);
    float rdiff = abs(r - pixel.y);
    float sdiff = abs(s - initS);

    if (qdiff > rdiff && qdiff > sdiff) {
        q = -r-s;
    } else if (rdiff > sdiff) {
        r = -q-s;
    }

    return float2(q, r);
}

float2 pixel_to_hex(float2 pt, float size) {
    return float2( 
        ( 2.0/3 * pt.x                     ) / size,
        (-1.0/3 * pt.x  +  sqrt(3)/3 * pt.y) / size
    );
}

float2 hex_to_pixel(float2 hex, float size) {
    float x = size * (     3.0 / 2 * hex.x                    );
    float y = size * (sqrt(3)  / 2 * hex.x  +  sqrt(3) * hex.y);
    return float2(x, y);
}
