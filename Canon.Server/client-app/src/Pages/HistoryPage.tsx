import {Box, Button, Card, CardActionArea, Drawer, Stack, Typography} from "@mui/material";
import createClient from "openapi-fetch";
import * as openapi from "../openapi";
import {useEffect, useState} from "react";
import {OutputIntf} from "../Interfaces/OutputIntf.ts";
import {useNavigate} from "react-router-dom";
import {enqueueSnackbar} from "notistack";

const client = createClient<openapi.paths>();


// @ts-expect-error ...
export function HistoryPage({state, setState}) {
    const [data, setData] = useState<OutputIntf[]>([]);
    const navigate = useNavigate();
    useEffect(() => {

        const getResponse = async () => {
            await client.GET("/api/Compiler", {
                params:
                    {
                        query: {
                            start: 1,
                            end: 20
                        }
                    }
            }).then((response) => {
                if (response !== undefined) {
                   // @ts-expect-error ...
                    setData(response.data)
                }
            });
        }
        getResponse();

    }, [data])

    const toggleDrawerClose = () => {
        setState(false);
    }

    const deleteHistory = async () => {
        await client.DELETE("/api/Compiler")
            .then((res) => {
                    if(res.response.status === 204) {
                        enqueueSnackbar("删除缓存成功", {variant: "success", anchorOrigin: {vertical: 'bottom', horizontal: 'right'}});
                        navigate('/');
                    } else {
                        enqueueSnackbar("删除缓存失败", {variant: "error", anchorOrigin: {vertical: 'bottom', horizontal: 'right'}});
                    }
                }
            );

    }

    return <>
        <Drawer
            anchor={"right"}
            open={state}
            onClose={toggleDrawerClose}
        >

            <Stack spacing={2}
                   sx={{height: "100%", overflowY: "scroll", padding: "5%"}}
                   direction="column">
                {
                    data.map((item, index) => {
                            return <Card key={index}
                                         sx={{width: "100%", height: "auto"}}>
                                <CardActionArea onClick={() => {
                                    console.log(item.id)
                                    navigate(`/${item.id}`)
                                }}>
                                    <Box sx={{padding: "5%"}}>
                                        <Typography variant="h5" gutterBottom>
                                            {item.compileTime}
                                        </Typography>
                                        {item.id}
                                    </Box>
                                </CardActionArea>
                            </Card>
                        }
                    )
                }
            </Stack>
            <Button sx={{width: "20rem"}} onClick={deleteHistory} size={"large"}>
                清除缓存
            </Button>
        </Drawer>
    </>
}
