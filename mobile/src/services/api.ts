import axios from 'axios'; import {API_BASE_URL} from '../constants/config'; import {AnalysisResult} from '../types/analysis';
const client=axios.create({baseURL:API_BASE_URL,timeout:60000});
export async function analyzePhoto(uri:string,type='image/jpeg'):Promise<AnalysisResult>{const form=new FormData();form.append('photo',{uri,name:'photo.jpg',type} as never);return (await client.post<AnalysisResult>('/api/analyze/photo',form,{headers:{'Content-Type':'multipart/form-data'}})).data;}
