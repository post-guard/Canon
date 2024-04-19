import {CSSProperties} from "react";
import {PhotoProvider, PhotoView} from "react-photo-view";


// @ts-expect-error ...
export function OutputField({imgPath}) {
    return <>
        <div className={"output-field"} style={outputFieldClassCss}>
            <PhotoProvider>
                    <PhotoView key={1} src={imgPath}>
                        <img src={imgPath}
                             style={{ objectFit: 'cover' ,width:"100%",height:"100%" }}
                             alt=""/>
                    </PhotoView>
            </PhotoProvider>
        </div>
    </>
}

const outputFieldClassCss: CSSProperties = {
    width: "100%",
    height: "100%",
    padding: "5% 5%",
    boxSizing: "border-box",
}


