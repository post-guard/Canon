import {CSSProperties, useState} from "react";
import MonacoEditor from "react-monaco-editor";


// @ts-expect-error ...
export function InputField({onValueChange}) {

    const [inputValue, setInputValue] = useState('');

    // @ts-expect-error ...
    const handleChange = (newValue) => {
        setInputValue(newValue);
        onValueChange(newValue);
    };


    return <>
        <div className={"input-field"} style={inputFieldClassCss}>
            <div style={{
                width: "100%",
                height: "100%",
                border: "0.1rem solid #1976d2"
            }}>
                <MonacoEditor
                    onChange={handleChange}
                    language="javascript"
                    theme="twilight"
                    value={inputValue}
                />
            </div>
        </div>
    </>
}

const inputFieldClassCss: CSSProperties = {
    width: "100%",
    height: "100%",
    padding: "5% 5%",
    boxSizing: "border-box",
}
