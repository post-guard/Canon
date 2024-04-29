import { CSSProperties, useState } from "react";
import { Box, ToggleButton, ToggleButtonGroup } from "@mui/material";
import { PhotoProvider, PhotoView } from "react-photo-view";
import MonacoEditor from "react-monaco-editor";
import { OutputIntf } from "../Interfaces/OutputIntf";

export function OutputField(props: OutputIntf) {
    const [state, setState] = useState('tree')
    const { imageAddress, compiledCode, compileInformation } = props.data;


    return <>
        <div className={"output-field"} style={outputFieldClassCss}>
            <ToggleButtonGroup
                color="primary"
                value={state}
                sx={{
                    position: "relative",
                    top: "0",
                    left: "50%",
                    height: "10%",
                    paddingBottom: "5%",
                    transform: "translateX(-50%)"
                }}
                exclusive
                onChange={(_event, value) => {
                    setState(value + "");
                }}
                aria-label="Platform"
            >
                <ToggleButton value="code"
                    aria-label="code"
                    size={"small"}>
                    Code
                </ToggleButton>
                <ToggleButton value="tree"
                    aria-label="tree"
                    size={"small"}>
                    Tree
                </ToggleButton>
                <ToggleButton value="log" aria-label="log" size={"small"}>
                    Log
                </ToggleButton>
            </ToggleButtonGroup>
            <Box sx={{
                height: "90%",
            }}>
                {
                    state === 'tree' &&
                    <PhotoProvider>
                        <PhotoView key={1} src={imageAddress}>
                            {imageAddress == "pic/uncompiled.png" ?
                                <img src={imageAddress}
                                    style={{
                                        width: "100%",
                                        height: "auto"
                                    }}
                                    alt="" /> :
                                <img src={imageAddress}
                                    style={{
                                        objectFit: 'cover',
                                        width: "100%",
                                        height: "100%"
                                    }}
                                    alt="" />
                            }

                        </PhotoView>
                    </PhotoProvider>
                }
                {
                    state == "code" && <MonacoEditor
                        language="javascript"
                        theme="twilight"
                        value={compiledCode === "" ? "也就是说,还没编译啊还没编译" : compiledCode}
                        options={{ readOnly: true }}
                    />
                }
                {
                    state == "log" && <MonacoEditor
                        theme={"twilight"}
                        value={compileInformation}
                        options={{readOnly: true}}
                    />
                }
            </Box>
        </div>
    </>
}

const outputFieldClassCss: CSSProperties = {
    width: "100%",
    height: "100%",
    padding: "5% 5%",
    boxSizing: "border-box",
}


